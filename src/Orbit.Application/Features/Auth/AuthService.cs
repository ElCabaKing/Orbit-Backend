using System.Security.Claims;
using System.Security.Cryptography;
using Orbit.Application.Common;
using Orbit.Application.DTOs;
using Orbit.Application.Enums;
using Orbit.Application.Interfaces;
using Orbit.Domain.Entities;

namespace Orbit.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<AuthUser> _authUserRepo;
    private readonly IGenericRepository<Profile> _profileRepo;
    private readonly IGenericRepository<UserSession> _sessionRepo;
    private readonly IGenericRepository<UserPrefix> _prefixRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IResetTokenService _resetTokenService;

    private const string TokenChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public AuthService(
        IGenericRepository<AuthUser> authUserRepo,
        IGenericRepository<Profile> profileRepo,
        IGenericRepository<UserSession> sessionRepo,
        IGenericRepository<UserPrefix> prefixRepo,
        IPasswordHasher passwordHasher,
        ICloudinaryService cloudinaryService,
        IJwtService jwtService,
        IEmailService emailService,
        IResetTokenService resetTokenService)
    {
        _authUserRepo = authUserRepo;
        _profileRepo = profileRepo;
        _sessionRepo = sessionRepo;
        _prefixRepo = prefixRepo;
        _passwordHasher = passwordHasher;
        _cloudinaryService = cloudinaryService;
        _jwtService = jwtService;
        _emailService = emailService;
        _resetTokenService = resetTokenService;
    }

    private static string GenerateResetToken()
    {
        return string.Create(6, TokenChars, (span, chars) =>
        {
            for (int i = 0; i < 6; i++)
                span[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        });
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(
        string email,
        string username,
        string displayName,
        string password,
        Stream? profilePictureStream,
        string? profilePictureFileName,
        string? bio)
    {
        var emailExists = await _authUserRepo.FirstOrDefaultAsync(u => u.Email == email);
        if (emailExists is not null)
            return Result<RegisterResponse>.Failure("Email is already registered");

        var usernameSlug = username.ToLowerInvariant();
        var usernameExists = await _profileRepo.FirstOrDefaultAsync(p => p.UsernameSlug == usernameSlug);
        if (usernameExists is not null)
            return Result<RegisterResponse>.Failure("Username is already taken");

        var passwordHash = _passwordHasher.Hash(password);
        var authUser = new AuthUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _authUserRepo.CreateAsync(authUser);

        string? avatarUrl = null;

        if (profilePictureStream is not null && !string.IsNullOrEmpty(profilePictureFileName))
        {
            var fileName = $"{authUser.Id}_{Guid.NewGuid()}";
            var uploadResult = await _cloudinaryService.UploadAsync(
                profilePictureStream, fileName, CloudinaryFolder.ProfilePics);

            if (uploadResult.IsSuccess)
            {
                avatarUrl = uploadResult.Data!.Url;
            }
        }

        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUser.Id,
            Username = username,
            UsernameSlug = usernameSlug,
            DisplayName = displayName,
            Bio = bio,
            ProfilePictureUrl = avatarUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _profileRepo.CreateAsync(profile);

        return Result<RegisterResponse>.Success(new RegisterResponse(
            authUser.Id, email, username, displayName, avatarUrl, bio
        ), "Registration successful");
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password)
    {
        var authUser = await _authUserRepo.FirstOrDefaultAsync(u => u.Email == email);
        if (authUser is null)
            return Result<AuthResponse>.Failure("Invalid credentials");

        if (!_passwordHasher.Verify(password, authUser.PasswordHash))
            return Result<AuthResponse>.Failure("Invalid credentials");

        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUser.Id);
        if (profile is null)
            return Result<AuthResponse>.Failure("Invalid credentials");

        var prefixResponse = await GetPrefixAsync(profile.PrefixId);

        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(authUser.Id, profile.Id, profile.Username);

        var rawRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _passwordHasher.Hash(rawRefreshToken);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUser.Id,
            RefreshTokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
        };

        await _sessionRepo.CreateAsync(session);

        var profileResponse = BuildProfileResponse(profile, prefixResponse);
        var response = new AuthResponse(accessToken, rawRefreshToken, expiresAt, profileResponse);
        return Result<AuthResponse>.Success(response, "Login successful");
    }

    public async Task<Result> LogoutAsync(string refreshToken)
    {
        var sessions = await _sessionRepo.GetAllAsync();

        UserSession? sessionToDelete = null;
        foreach (var session in sessions)
        {
            if (_passwordHasher.Verify(refreshToken, session.RefreshTokenHash))
            {
                sessionToDelete = session;
                break;
            }
        }

        if (sessionToDelete is not null)
        {
            await _sessionRepo.DeleteAsync(sessionToDelete.Id);
        }

        return Result.Success("Logged out successfully");
    }

    public async Task<Result<ProfileResponse>> GetCurrentUserAsync(Guid authUserId)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<ProfileResponse>.Failure("Profile not found");

        var prefixResponse = await GetPrefixAsync(profile.PrefixId);
        var profileResponse = BuildProfileResponse(profile, prefixResponse);

        return Result<ProfileResponse>.Success(profileResponse);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
        if (principal is null)
            return Result<AuthResponse>.Failure("Invalid or expired token");

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? principal.FindFirst("sub")?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var authUserId))
            return Result<AuthResponse>.Failure("Invalid or expired token");

        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<AuthResponse>.Failure("Invalid or expired token");

        var sessions = await _sessionRepo.GetListAsync(s => s.AuthUserId == authUserId);

        UserSession? validSession = null;
        foreach (var session in sessions)
        {
            if (_passwordHasher.Verify(refreshToken, session.RefreshTokenHash))
            {
                validSession = session;
                break;
            }
        }

        if (validSession is null)
            return Result<AuthResponse>.Failure("Invalid refresh token");

        if (validSession.ExpiresAt < DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Session expired");

        await _sessionRepo.DeleteAsync(validSession.Id);

        var prefixResponse = await GetPrefixAsync(profile.PrefixId);

        var (newAccessToken, expiresAt) = _jwtService.GenerateAccessToken(authUserId, profile.Id, profile.Username);

        var rawRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _passwordHasher.Hash(rawRefreshToken);

        var newSession = new UserSession
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            RefreshTokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
        };

        await _sessionRepo.CreateAsync(newSession);

        var profileResponse = BuildProfileResponse(profile, prefixResponse);
        var response = new AuthResponse(newAccessToken, rawRefreshToken, expiresAt, profileResponse);
        return Result<AuthResponse>.Success(response, "Token refreshed successfully");
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var authUser = await _authUserRepo.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (authUser is not null)
        {
            var token = GenerateResetToken();
            await _resetTokenService.SaveTokenAsync(normalizedEmail, token, TimeSpan.FromMinutes(15));

            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000";
            var resetUrl = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(normalizedEmail)}";

            var htmlBody = $"""
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>Your reset token is: <strong>{token}</strong></p>
                <p>This token will expire in 15 minutes.</p>
                <p>
                    <a href="{resetUrl}">Click here to reset your password</a>
                </p>
                <p>If you did not request this, please ignore this email.</p>
            </body>
            </html>
            """;

            await _emailService.SendAsync(normalizedEmail, "", "Orbit - Password Reset", htmlBody);
        }

        return Result.Success("If registered, check your inbox");
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var storedToken = await _resetTokenService.GetTokenAsync(normalizedEmail);

        if (storedToken is null || storedToken != token)
            return Result.Failure("Invalid or expired token");

        var authUser = await _authUserRepo.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (authUser is null)
            return Result.Failure("Invalid or expired token");

        authUser.PasswordHash = _passwordHasher.Hash(newPassword);
        authUser.UpdatedAt = DateTime.UtcNow;
        _authUserRepo.Update(authUser);
        await _authUserRepo.SaveChangesAsync();

        await _resetTokenService.RemoveTokenAsync(normalizedEmail);

        return Result.Success("Password reset successful");
    }

    private async Task<UserPrefixResponse?> GetPrefixAsync(Guid? prefixId)
    {
        if (!prefixId.HasValue) return null;

        var prefix = await _prefixRepo.GetByIdAsync(prefixId.Value);
        return prefix is null ? null : new UserPrefixResponse(prefix.Id, prefix.Name, prefix.Color, prefix.IconUrl);
    }

    private static ProfileResponse BuildProfileResponse(Profile profile, UserPrefixResponse? prefix)
    {
        return new ProfileResponse(
            profile.Id,
            profile.Username,
            profile.DisplayName,
            profile.ProfilePictureUrl,
            profile.BannerUrl,
            profile.Bio,
            profile.FollowersCount,
            profile.FollowingCount,
            profile.IsVerified,
            prefix
        );
    }
}

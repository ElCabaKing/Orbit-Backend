using System.Security.Claims;
using System.Security.Cryptography;
using Orbit.Application.Common;
using Orbit.Application.Constants;
using Orbit.Application.DTOs;
using Orbit.Application.Enums;
using Orbit.Application.Interfaces;
using Orbit.Domain.Entities;
using Orbit.Shared.Constants;

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
            return Result<RegisterResponse>.Failure(ResponseMessages.EmailAlreadyRegistered);

        var usernameSlug = username.ToLowerInvariant();
        var usernameExists = await _profileRepo.FirstOrDefaultAsync(p => p.UsernameSlug == usernameSlug);
        if (usernameExists is not null)
            return Result<RegisterResponse>.Failure(ResponseMessages.UsernameAlreadyTaken);

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
        ), ResponseMessages.RegistrationSuccessful);
    }

    public async Task<Result<AuthResponse>> LoginAsync(string emailOrUsername, string password)
    {
        var authUser = await _authUserRepo.FirstOrDefaultAsync(u => u.Email == emailOrUsername);

        if (authUser is null)
        {
            var profileByUsername = await _profileRepo.FirstOrDefaultAsync(p => p.Username == emailOrUsername);
            if (profileByUsername is not null)
                authUser = await _authUserRepo.FirstOrDefaultAsync(u => u.Id == profileByUsername.AuthUserId);
        }

        if (authUser is null)
            return Result<AuthResponse>.Failure(ResponseMessages.InvalidCredentials);

        if (!_passwordHasher.Verify(password, authUser.PasswordHash))
            return Result<AuthResponse>.Failure(ResponseMessages.InvalidCredentials);

        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUser.Id);
        if (profile is null)
            return Result<AuthResponse>.Failure(ResponseMessages.InvalidCredentials);

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
        return Result<AuthResponse>.Success(response, ResponseMessages.LoginSuccessful);
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

        return Result.Success(ResponseMessages.LoggedOutSuccessfully);
    }

    public async Task<Result<ProfileResponse>> GetCurrentUserAsync(Guid authUserId)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<ProfileResponse>.Failure(ResponseMessages.ProfileNotFound);

        var prefixResponse = await GetPrefixAsync(profile.PrefixId);
        var profileResponse = BuildProfileResponse(profile, prefixResponse);

        return Result<ProfileResponse>.Success(profileResponse);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
        if (principal is null)
            return Result<AuthResponse>.Failure(ResponseMessages.InvalidOrExpiredToken);

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? principal.FindFirst(ClaimConstants.Sub)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var authUserId))
            return Result<AuthResponse>.Failure(ResponseMessages.InvalidOrExpiredToken);

        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<AuthResponse>.Failure(ResponseMessages.InvalidOrExpiredToken);

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
            return Result<AuthResponse>.Failure(ResponseMessages.InvalidRefreshToken);

        if (validSession.ExpiresAt < DateTime.UtcNow)
            return Result<AuthResponse>.Failure(ResponseMessages.SessionExpired);

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
        return Result<AuthResponse>.Success(response, ResponseMessages.TokenRefreshed);
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var authUser = await _authUserRepo.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (authUser is not null)
        {
            var token = GenerateResetToken();
            await _resetTokenService.SaveTokenAsync(normalizedEmail, token, TimeSpan.FromMinutes(15));

            var frontendUrl = Environment.GetEnvironmentVariable(EnvironmentConstants.FrontendUrl) ?? DefaultsConstants.FrontendUrl;
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

        return Result.Success(ResponseMessages.CheckYourInbox);
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var storedToken = await _resetTokenService.GetTokenAsync(normalizedEmail);

        if (storedToken is null || storedToken != token)
            return Result.Failure(ResponseMessages.InvalidOrExpiredToken);

        var authUser = await _authUserRepo.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (authUser is null)
            return Result.Failure(ResponseMessages.InvalidOrExpiredToken);

        authUser.PasswordHash = _passwordHasher.Hash(newPassword);
        authUser.UpdatedAt = DateTime.UtcNow;
        _authUserRepo.Update(authUser);
        await _authUserRepo.SaveChangesAsync();

        await _resetTokenService.RemoveTokenAsync(normalizedEmail);

        return Result.Success(ResponseMessages.PasswordResetSuccessful);
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

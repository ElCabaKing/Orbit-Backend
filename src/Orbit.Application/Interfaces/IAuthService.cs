using Orbit.Application.Common;
using Orbit.Application.DTOs;

namespace Orbit.Application.Interfaces;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(
        string email,
        string username,
        string displayName,
        string password,
        Stream? profilePictureStream,
        string? profilePictureFileName,
        string? bio);

    Task<Result<AuthResponse>> LoginAsync(string email, string password);
    Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken);
    Task<Result> LogoutAsync(string refreshToken);
    Task<Result<ProfileResponse>> GetCurrentUserAsync(Guid authUserId);
}

namespace Orbit.Application.Interfaces;

public interface IResetTokenService
{
    Task SaveTokenAsync(string email, string token, TimeSpan expiration);
    Task<string?> GetTokenAsync(string email);
    Task RemoveTokenAsync(string email);
}

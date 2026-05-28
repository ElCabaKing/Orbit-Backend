namespace Orbit.ApiWeb.DTOs;

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

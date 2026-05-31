namespace Orbit.ApiWeb.DTOs;

public record ResetPasswordRequest(string Username, string Token, string NewPassword);

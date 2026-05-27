namespace Orbit.ApiWeb.DTOs;

public record LoginRequest(
    string Email,
    string Password
);

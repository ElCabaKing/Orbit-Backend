namespace Orbit.ApiWeb.DTOs;

public record LoginRequest(
    string EmailOrUsername,
    string Password
);

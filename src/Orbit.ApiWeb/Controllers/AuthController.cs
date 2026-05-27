using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbit.ApiWeb.DTOs;
using Orbit.Application.Interfaces;

namespace Orbit.ApiWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterRequest request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { isSuccess = false, message = "Validation failed", errors });
        }

        Stream? fileStream = null;
        if (request.ProfilePicture is not null)
        {
            fileStream = request.ProfilePicture.OpenReadStream();
        }

        var result = await _authService.RegisterAsync(
            request.Email,
            request.Username,
            request.DisplayName,
            request.Password,
            fileStream,
            request.ProfilePicture?.FileName,
            request.Bio
        );

        if (!result.IsSuccess)
        {
            return result.Message switch
            {
                "Email is already registered" => Conflict(new { isSuccess = false, message = result.Message }),
                "Username is already taken" => Conflict(new { isSuccess = false, message = result.Message }),
                _ => StatusCode(500, new { isSuccess = false, message = result.Message }),
            };
        }

        return CreatedAtAction(nameof(Register), new { isSuccess = true, message = result.Message, data = result.Data });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { isSuccess = false, message = "Validation failed", errors });
        }

        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { isSuccess = false, message = result.Message });
        }

        return Ok(new { isSuccess = true, message = result.Message, data = result.Data });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var result = await _authService.LogoutAsync(request.RefreshToken);
        return Ok(new { isSuccess = true, message = result.Message });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var authUserId))
            return Unauthorized(new { isSuccess = false, message = "Invalid token" });

        var result = await _authService.GetCurrentUserAsync(authUserId);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);

        if (!result.IsSuccess)
        {
            return result.Message switch
            {
                "Session expired" => Unauthorized(new { isSuccess = false, message = result.Message }),
                _ => Unauthorized(new { isSuccess = false, message = result.Message }),
            };
        }

        return Ok(new { isSuccess = true, message = result.Message, data = result.Data });
    }
}

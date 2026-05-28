using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbit.ApiWeb.DTOs;
using Orbit.Application.Constants;
using Orbit.Application.Interfaces;

namespace Orbit.ApiWeb.Controllers;

[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;

    public ProfileController(
        IProfileService profileService,
        IValidator<UpdateProfileRequest> updateProfileValidator)
    {
        _profileService = profileService;
        _updateProfileValidator = updateProfileValidator;
    }

    [AllowAnonymous]
    [HttpGet("api/profiles/{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        var result = await _profileService.GetProfileByUsernameAsync(username);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpPut("api/profile")]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
    {
        var validationResult = await _updateProfileValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { isSuccess = false, message = ResponseMessages.ValidationFailed, errors });
        }

        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _profileService.UpdateProfileAsync(authUserId.Value, request.DisplayName, request.Bio, request.IsPrivate);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpPut("api/profile/avatar")]
    public async Task<IActionResult> UpdateAvatar(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { isSuccess = false, message = ResponseMessages.FileRequired });

        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        using var stream = file.OpenReadStream();
        var result = await _profileService.UpdateProfilePictureAsync(authUserId.Value, stream, file.FileName);

        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpDelete("api/profile/avatar")]
    public async Task<IActionResult> RemoveAvatar()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _profileService.RemoveProfilePictureAsync(authUserId.Value);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpPut("api/profile/banner")]
    public async Task<IActionResult> UpdateBanner(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { isSuccess = false, message = ResponseMessages.FileRequired });

        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        using var stream = file.OpenReadStream();
        var result = await _profileService.UpdateBannerAsync(authUserId.Value, stream, file.FileName);

        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpDelete("api/profile/banner")]
    public async Task<IActionResult> RemoveBanner()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _profileService.RemoveBannerAsync(authUserId.Value);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    private Guid? GetAuthUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(ClaimConstants.Sub)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var authUserId))
            return null;
        return authUserId;
    }
}

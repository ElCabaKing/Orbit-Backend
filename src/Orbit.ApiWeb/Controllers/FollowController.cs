using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbit.Application.Constants;
using Orbit.Application.Interfaces;

namespace Orbit.ApiWeb.Controllers;

[ApiController]
public class FollowController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowController(IFollowService followService)
    {
        _followService = followService;
    }

    [Authorize]
    [HttpPost("api/profiles/{username}/follow")]
    public async Task<IActionResult> Follow(string username)
    {
        var profileId = GetProfileId();
        if (profileId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _followService.FollowUserAsync(profileId.Value, username);

        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, message = result.Message });
    }

    [Authorize]
    [HttpDelete("api/profiles/{username}/follow")]
    public async Task<IActionResult> Unfollow(string username)
    {
        var profileId = GetProfileId();
        if (profileId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _followService.UnfollowUserAsync(profileId.Value, username);

        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, message = result.Message });
    }

    [AllowAnonymous]
    [HttpGet("api/profiles/{username}/followers")]
    public async Task<IActionResult> GetFollowers(string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentProfileId = GetProfileId();
        var result = await _followService.GetFollowersAsync(username, currentProfileId, page, Math.Clamp(pageSize, 1, 100));

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [AllowAnonymous]
    [HttpGet("api/profiles/{username}/following")]
    public async Task<IActionResult> GetFollowing(string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentProfileId = GetProfileId();
        var result = await _followService.GetFollowingAsync(username, currentProfileId, page, Math.Clamp(pageSize, 1, 100));

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    private Guid? GetProfileId()
    {
        var profileIdClaim = User.FindFirst(ClaimConstants.ProfileId)?.Value;
        if (profileIdClaim is null || !Guid.TryParse(profileIdClaim, out var profileId))
            return null;
        return profileId;
    }
}

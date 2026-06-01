using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbit.Application.Constants;
using Orbit.Application.Interfaces;

namespace Orbit.ApiWeb.Controllers;

public class FollowController : BaseController
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

}

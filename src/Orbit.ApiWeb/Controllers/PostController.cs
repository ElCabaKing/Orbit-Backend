using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbit.ApiWeb.DTOs;
using Orbit.Application.Constants;
using Orbit.Application.Interfaces;

namespace Orbit.ApiWeb.Controllers;

[ApiController]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IValidator<CreatePostRequest> _createPostValidator;
    private readonly IValidator<CreateCommentRequest> _createCommentValidator;

    public PostController(
        IPostService postService,
        IValidator<CreatePostRequest> createPostValidator,
        IValidator<CreateCommentRequest> createCommentValidator)
    {
        _postService = postService;
        _createPostValidator = createPostValidator;
        _createCommentValidator = createCommentValidator;
    }

    [Authorize]
    [HttpPost("api/posts")]
    public async Task<IActionResult> Create([FromForm] CreatePostRequest request)
    {
        var validationResult = await _createPostValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { isSuccess = false, message = ResponseMessages.ValidationFailed, errors });
        }

        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        Stream? mediaStream = null;
        if (request.Media is not null)
        {
            mediaStream = request.Media.OpenReadStream();
        }

        var result = await _postService.CreatePostAsync(
            authUserId.Value, request.Content, mediaStream, request.Media?.FileName);

        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return CreatedAtAction(nameof(Create), new { isSuccess = true, data = result.Data });
    }

    [AllowAnonymous]
    [HttpGet("api/posts/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var currentProfileId = GetProfileId();
        var result = await _postService.GetPostAsync(id, currentProfileId);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpGet("api/posts/timeline")]
    public async Task<IActionResult> GetTimeline([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentProfileId = GetProfileId();
        var result = await _postService.GetTimelineAsync(currentProfileId, page, Math.Clamp(pageSize, 1, 100));

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [AllowAnonymous]
    [HttpGet("api/profiles/{username}/posts")]
    public async Task<IActionResult> GetProfilePosts(string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentProfileId = GetProfileId();
        var result = await _postService.GetProfilePostsAsync(username, currentProfileId, page, Math.Clamp(pageSize, 1, 100));

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpPut("api/posts/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > 1000)
            return BadRequest(new { isSuccess = false, message = "Content is required and must not exceed 1000 characters" });

        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _postService.UpdatePostAsync(authUserId.Value, id, request.Content);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpDelete("api/posts/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _postService.DeletePostAsync(authUserId.Value, id);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, message = result.Message });
    }

    [Authorize]
    [HttpPost("api/posts/{id:guid}/like")]
    public async Task<IActionResult> Like(Guid id)
    {
        var profileId = GetProfileId();
        if (profileId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _postService.LikePostAsync(profileId.Value, id);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpDelete("api/posts/{id:guid}/like")]
    public async Task<IActionResult> Unlike(Guid id)
    {
        var profileId = GetProfileId();
        if (profileId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _postService.UnlikePostAsync(profileId.Value, id);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpPost("api/posts/{id:guid}/comments")]
    public async Task<IActionResult> CreateComment(Guid id, [FromBody] CreateCommentRequest request)
    {
        var validationResult = await _createCommentValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { isSuccess = false, message = ResponseMessages.ValidationFailed, errors });
        }

        var profileId = GetProfileId();
        if (profileId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _postService.CreateCommentAsync(profileId.Value, id, request.Content);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return CreatedAtAction(nameof(CreateComment), new { isSuccess = true, data = result.Data });
    }

    [AllowAnonymous]
    [HttpGet("api/posts/{id:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _postService.GetCommentsAsync(id, page, Math.Clamp(pageSize, 1, 100));

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpDelete("api/comments/{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _postService.DeleteCommentAsync(authUserId.Value, id);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, message = result.Message });
    }

    private Guid? GetAuthUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(ClaimConstants.Sub)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var authUserId))
            return null;
        return authUserId;
    }

    private Guid? GetProfileId()
    {
        var profileIdClaim = User.FindFirst(ClaimConstants.ProfileId)?.Value;
        if (profileIdClaim is null || !Guid.TryParse(profileIdClaim, out var profileId))
            return null;
        return profileId;
    }
}

using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbit.ApiWeb.Constants;
using Orbit.ApiWeb.DTOs;
using Orbit.Application.Common;
using Orbit.Application.Constants;
using Orbit.Application.DTOs;
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
    [EndpointSummary("Crear publicación")]
    [EndpointDescription("Crea una nueva publicación con contenido opcional y archivos multimedia.")]
    [ProducesResponseType<Result<PostResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
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

        List<MediaUploadData>? mediaFiles = null;
        if (request.Media is not null && request.Media.Count > 0)
        {
            mediaFiles = request.Media
                .Where(f => f is not null)
                .Select(f => new MediaUploadData(f.OpenReadStream(), f.FileName))
                .ToList();
        }

        var result = await _postService.CreatePostAsync(
            authUserId.Value, request.Content, mediaFiles);

        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return CreatedAtAction(nameof(Create), null, new { isSuccess = true, data = result.Data });
    }

    [AllowAnonymous]
    [HttpGet("api/posts/{id:guid}")]
    [EndpointSummary("Obtener publicación")]
    [EndpointDescription("Obtiene una publicación por su ID.")]
    [ProducesResponseType<Result<PostResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status404NotFound)]
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
    [EndpointSummary("Timeline")]
    [EndpointDescription("Obtiene el timeline con publicaciones de los usuarios seguidos.")]
    [ProducesResponseType<Result<PagedResult<PostResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTimeline([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentProfileId = GetProfileId();
        var result = await _postService.GetTimelineAsync(currentProfileId, page, Math.Clamp(pageSize, 1, 100));

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [AllowAnonymous]
    [HttpGet("api/profiles/{username}/posts")]
    [EndpointSummary("Publicaciones de perfil")]
    [EndpointDescription("Obtiene las publicaciones de un perfil específico por username.")]
    [ProducesResponseType<Result<PagedResult<PostResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status404NotFound)]
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
    [EndpointSummary("Actualizar publicación")]
    [EndpointDescription("Actualiza el contenido y/o multimedia de una publicación existente.")]
    [ProducesResponseType<Result<PostResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Result>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdatePostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > 1000)
            return BadRequest(new { isSuccess = false, message = ValidationConstants.ContentRequiredAndMaxLength });

        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        List<MediaUploadData>? mediaFiles = null;
        if (request.Media is not null && request.Media.Count > 0)
        {
            mediaFiles = request.Media
                .Where(f => f is not null)
                .Select(f => new MediaUploadData(f.OpenReadStream(), f.FileName))
                .ToList();
        }

        var result = await _postService.UpdatePostAsync(authUserId.Value, id, request.Content, mediaFiles);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpDelete("api/posts/{id:guid}")]
    [EndpointSummary("Eliminar publicación")]
    [EndpointDescription("Elimina una publicación existente del usuario autenticado.")]
    [ProducesResponseType<Result>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Result>(StatusCodes.Status404NotFound)]
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
    [EndpointSummary("Dar like")]
    [EndpointDescription("Da like a una publicación.")]
    [ProducesResponseType<Result<LikeResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Result>(StatusCodes.Status404NotFound)]
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
    [EndpointSummary("Quitar like")]
    [EndpointDescription("Quita el like de una publicación.")]
    [ProducesResponseType<Result<LikeResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Result>(StatusCodes.Status404NotFound)]
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
    [EndpointSummary("Crear comentario")]
    [EndpointDescription("Crea un comentario en una publicación.")]
    [ProducesResponseType<Result<CommentResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Result>(StatusCodes.Status404NotFound)]
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

        var result = await _postService.CreateCommentAsync(profileId.Value, id, request.Content, request.ParentCommentId);

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return CreatedAtAction(nameof(CreateComment), null, new { isSuccess = true, data = result.Data });
    }

    [AllowAnonymous]
    [HttpGet("api/posts/{id:guid}/comments")]
    [EndpointSummary("Obtener comentarios")]
    [EndpointDescription("Obtiene los comentarios de una publicación.")]
    [ProducesResponseType<Result<PagedResult<CommentResponse>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _postService.GetCommentsAsync(id, page, Math.Clamp(pageSize, 1, 100));

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [AllowAnonymous]
    [HttpGet("api/comments/{id:guid}/replies")]
    [EndpointSummary("Obtener respuestas de comentario")]
    [EndpointDescription("Obtiene las respuestas paginadas de un comentario específico.")]
    [ProducesResponseType<Result<PagedResult<CommentResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentReplies(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _postService.GetCommentRepliesAsync(id, page, Math.Clamp(pageSize, 1, 100));

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [Authorize]
    [HttpDelete("api/comments/{id:guid}")]
    [EndpointSummary("Eliminar comentario")]
    [EndpointDescription("Elimina un comentario existente.")]
    [ProducesResponseType<Result>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Result>(StatusCodes.Status404NotFound)]
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

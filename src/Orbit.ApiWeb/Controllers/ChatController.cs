using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Orbit.ApiWeb.DTOs;
using Orbit.ApiWeb.Hubs;
using Orbit.Application.Common;
using Orbit.Application.Constants;
using Orbit.Application.DTOs;
using Orbit.Application.Interfaces;

namespace Orbit.ApiWeb.Controllers;

[Authorize]
public class ChatController : BaseController
{
    private readonly IChatService _chatService;
    private readonly IValidator<CreateChatRequest> _createChatValidator;
    private readonly IValidator<SendMessageRequest> _sendMessageValidator;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(
        IChatService chatService,
        IValidator<CreateChatRequest> createChatValidator,
        IValidator<SendMessageRequest> sendMessageValidator,
        IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _createChatValidator = createChatValidator;
        _sendMessageValidator = sendMessageValidator;
        _hubContext = hubContext;
    }

    [HttpPost("api/chats")]
    [EndpointSummary("Crear conversación")]
    [EndpointDescription("Crea una nueva conversación con otro usuario.")]
    [ProducesResponseType<Result<ChatResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateConversation([FromBody] CreateChatRequest request)
    {
        var validationResult = await _createChatValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(new
            {
                isSuccess = false,
                message = ResponseMessages.ValidationFailed,
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });

        var profileId = GetProfileId();
        if (profileId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _chatService.CreateConversationAsync(profileId.Value, request.Username);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        var otherProfileId = result.Data!.OtherParticipant.ProfileId;

        await _hubContext.Clients
            .Group($"profile:{otherProfileId}")
            .SendAsync("NewConversation", result.Data);

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [HttpGet("api/chats")]
    [EndpointSummary("Listar conversaciones")]
    [EndpointDescription("Obtiene las conversaciones del usuario autenticado.")]
    [ProducesResponseType<Result<PagedResult<ChatResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConversations(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var profileId = GetProfileId();
        if (profileId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _chatService.GetConversationsAsync(
            profileId.Value, page, Math.Clamp(pageSize, 1, 50));

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [HttpGet("api/chats/{conversationId}/messages")]
    [EndpointSummary("Obtener mensajes")]
    [EndpointDescription("Obtiene los mensajes de una conversación.")]
    [ProducesResponseType<Result<PagedResult<MessageResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Result>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var profileId = GetProfileId();
        if (profileId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _chatService.GetMessagesAsync(
            profileId.Value, conversationId, page, Math.Clamp(pageSize, 1, 100));

        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [HttpPost("api/chats/{conversationId}/messages")]
    [EndpointSummary("Enviar mensaje")]
    [EndpointDescription("Envía un mensaje en una conversación.")]
    [ProducesResponseType<Result<MessageResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request)
    {
        var validationResult = await _sendMessageValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(new
            {
                isSuccess = false,
                message = ResponseMessages.ValidationFailed,
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });

        var profileId = GetProfileId();
        if (profileId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _chatService.SendMessageAsync(profileId.Value, conversationId, request.Content);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        await _hubContext.Clients
            .Group($"conversation:{conversationId}")
            .SendAsync("ReceiveMessage", new
            {
                id = result.Data!.Id,
                conversationId = result.Data.ConversationId,
                senderProfileId = result.Data.SenderProfileId,
                content = result.Data.Content,
                isSeen = result.Data.IsSeen,
                isEdited = result.Data.IsEdited,
                createdAt = result.Data.CreatedAt,
            });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    [HttpDelete("api/chats/{conversationId}/messages/{messageId}")]
    [EndpointSummary("Eliminar mensaje")]
    [EndpointDescription("Elimina un mensaje específico de una conversación.")]
    [ProducesResponseType<Result>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteMessage(Guid conversationId, Guid messageId)
    {
        var profileId = GetProfileId();
        if (profileId is null)
            return Unauthorized(new { isSuccess = false, message = ResponseMessages.InvalidToken });

        var result = await _chatService.DeleteMessageAsync(profileId.Value, conversationId, messageId);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, message = result.Message });
    }

}

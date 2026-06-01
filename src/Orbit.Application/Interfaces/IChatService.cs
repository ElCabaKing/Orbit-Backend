using Orbit.Application.Common;
using Orbit.Application.DTOs;

namespace Orbit.Application.Interfaces;

public interface IChatService
{
    Task<Result<ChatResponse>> CreateConversationAsync(Guid currentProfileId, string targetUsername);
    Task<Result<PagedResult<ChatResponse>>> GetConversationsAsync(Guid currentProfileId, int page, int pageSize);
    Task<Result<PagedResult<MessageResponse>>> GetMessagesAsync(Guid currentProfileId, Guid conversationId, int page, int pageSize);
    Task<Result<MessageResponse>> SendMessageAsync(Guid currentProfileId, Guid conversationId, string content);
    Task<Result> DeleteMessageAsync(Guid currentProfileId, Guid conversationId, Guid messageId);
}

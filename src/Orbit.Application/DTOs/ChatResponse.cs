namespace Orbit.Application.DTOs;

public record ChatProfileInfo(
    Guid ProfileId,
    string Username,
    string DisplayName,
    string? AvatarUrl
);

public record MessageResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderProfileId,
    string? Content,
    bool IsSeen,
    bool IsEdited,
    DateTime? EditedAt,
    DateTime CreatedAt,
    DateTime? DeletedAt
);

public record ChatResponse(
    Guid Id,
    ChatProfileInfo OtherParticipant,
    MessageResponse? LastMessage,
    int UnreadCount,
    DateTime CreatedAt
);

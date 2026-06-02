namespace Orbit.Application.Constants;

public static class ResponseMessages
{
    public const string EmailAlreadyRegistered = "Email is already registered";
    public const string UsernameAlreadyTaken = "Username is already taken";
    public const string RegistrationSuccessful = "Registration successful";

    public const string InvalidCredentials = "Invalid credentials";
    public const string LoginSuccessful = "Login successful";

    public const string LoggedOutSuccessfully = "Logged out successfully";

    public const string TokenRefreshed = "Token refreshed successfully";
    public const string InvalidRefreshToken = "Invalid refresh token";
    public const string SessionExpired = "Session expired";
    public const string InvalidOrExpiredToken = "Invalid or expired token";

    public const string ProfileNotFound = "Profile not found";
    public const string FailedToUploadProfilePicture = "Failed to upload profile picture";
    public const string FailedToUploadBanner = "Failed to upload banner";

    public const string CheckYourInbox = "If registered, check your inbox";
    public const string PasswordResetSuccessful = "Password reset successful";

    public const string WelcomeEmailSent = "Welcome email sent";

    public const string ValidationFailed = "Validation failed";
    public const string InvalidToken = "Invalid token";
    public const string FileRequired = "File is required";

    public const string PostNotFound = "Post not found";
    public const string PostDeleted = "Post deleted successfully";
    public const string PostUpdated = "Post updated successfully";
    public const string CommentNotFound = "Comment not found";
    public const string CommentDeleted = "Comment deleted successfully";
    public const string NotAuthorized = "Not authorized";

    public const string CannotFollowYourself = "Cannot follow yourself";
    public const string AlreadyFollowing = "Already following this user";
    public const string NotFollowing = "You are not following this user";
    public const string FollowSuccessful = "Follow successful";
    public const string UnfollowSuccessful = "Unfollow successful";

    // Chat
    public const string MutualFollowRequired = "Both users must follow each other to start a chat";
    public const string CannotChatYourself = "Cannot start a chat with yourself";
    public const string ConversationNotFound = "Conversation not found";
    public const string MessageNotFound = "Message not found";
    public const string MessageDeleted = "Message deleted successfully";
    public const string NotConversationParticipant = "You are not a participant in this conversation";
    public const string NotMessageOwner = "You can only delete your own messages";
    public const string MessageContentRequired = "Message content is required";
    public const string MessageContentMaxLength = "Message content must not exceed 2000 characters";

    // Roles
    public const string UserAlreadyModerator = "User is already a moderator";
    public const string UserNotModerator = "User is not a moderator";
    public const string OnlyAdminCanAssignRoles = "Only admins can assign roles";
    public const string RoleAssigned = "Role assigned successfully";
    public const string RoleRemoved = "Role removed successfully";

    // Ban
    public const string AccountBanned = "Your account has been banned";
    public const string AccountDeactivated = "Your account has been deactivated";
    public const string UserAlreadyBanned = "User is already banned";
    public const string UserNotBanned = "User is not banned";
    public const string BanSuccessful = "User banned successfully";
    public const string UnbanSuccessful = "User unbanned successfully";
    public const string CannotBanYourself = "Cannot ban yourself";
    public const string CannotBanAdmin = "Cannot ban an admin user";

    // Saved Posts
    public const string PostAlreadySaved = "Post is already saved";
    public const string PostNotSaved = "Post is not saved";
    public const string PostSaved = "Post saved successfully";
    public const string PostUnsaved = "Post unsaved successfully";

    // Moderator
    public const string NotAuthorizedModerator = "Only moderators or admins can perform this action";

    // Comments
    public const string ParentCommentNotFound = "Parent comment not found";
    public const string ParentCommentNotInSamePost = "Parent comment does not belong to this post";

    // Block
    public const string CannotBlockYourself = "Cannot block yourself";
    public const string AlreadyBlocked = "User is already blocked";
    public const string BlockedByUser = "Cannot block this user because they have blocked you";
    public const string NotBlocked = "User is not blocked";
    public const string BlockSuccessful = "User blocked successfully";
    public const string UnblockSuccessful = "User unblocked successfully";
}

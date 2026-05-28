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
}

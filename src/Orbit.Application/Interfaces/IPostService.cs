using Orbit.Application.Common;
using Orbit.Application.DTOs;

namespace Orbit.Application.Interfaces;

public interface IPostService
{
    Task<Result<PostResponse>> CreatePostAsync(Guid authUserId, string content, List<MediaUploadData>? mediaFiles);
    Task<Result<PostResponse>> GetPostAsync(Guid postId, Guid? currentProfileId);
    Task<Result<PagedResult<PostResponse>>> GetTimelineAsync(Guid? currentProfileId, int page, int pageSize);
    Task<Result<PagedResult<PostResponse>>> GetProfilePostsAsync(string username, Guid? currentProfileId, int page, int pageSize);
    Task<Result<PostResponse>> UpdatePostAsync(Guid authUserId, Guid postId, string content, List<MediaUploadData>? mediaFiles = null);
    Task<Result> DeletePostAsync(Guid authUserId, Guid postId);
    Task<Result<LikeResponse>> LikePostAsync(Guid profileId, Guid postId);
    Task<Result<LikeResponse>> UnlikePostAsync(Guid profileId, Guid postId);
    Task<Result<CommentResponse>> CreateCommentAsync(Guid profileId, Guid postId, string content);
    Task<Result<PagedResult<CommentResponse>>> GetCommentsAsync(Guid postId, int page, int pageSize);
    Task<Result> DeleteCommentAsync(Guid authUserId, Guid commentId);
}

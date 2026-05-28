using Orbit.Application.Common;
using Orbit.Application.DTOs;

namespace Orbit.Application.Interfaces;

public interface IFollowService
{
    Task<Result> FollowUserAsync(Guid followerProfileId, string username);
    Task<Result> UnfollowUserAsync(Guid followerProfileId, string username);
    Task<Result<PagedResult<PostAuthorResponse>>> GetFollowersAsync(string username, Guid? currentProfileId, int page, int pageSize);
    Task<Result<PagedResult<PostAuthorResponse>>> GetFollowingAsync(string username, Guid? currentProfileId, int page, int pageSize);
}

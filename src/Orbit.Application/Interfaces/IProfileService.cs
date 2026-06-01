using Orbit.Application.Common;
using Orbit.Application.DTOs;

namespace Orbit.Application.Interfaces;

public interface IProfileService
{
    Task<Result<ProfileResponse>> GetProfileByUsernameAsync(string username, Guid? currentProfileId = null);
    Task<Result<ProfileResponse>> UpdateProfileAsync(Guid authUserId, string? displayName, string? bio, bool? isPrivate);
    Task<Result<ProfileResponse>> UpdateProfilePictureAsync(Guid authUserId, Stream fileStream, string fileName);
    Task<Result<ProfileResponse>> RemoveProfilePictureAsync(Guid authUserId);
    Task<Result<ProfileResponse>> UpdateBannerAsync(Guid authUserId, Stream fileStream, string fileName);
    Task<Result<ProfileResponse>> RemoveBannerAsync(Guid authUserId);
    Task<Result<PagedResult<SearchProfileResponse>>> SearchProfilesAsync(string query, Guid? currentProfileId, int page, int pageSize);

    Task<Result> BlockUserAsync(Guid blockerProfileId, string username);
    Task<Result> UnblockUserAsync(Guid blockerProfileId, string username);
    Task<Result<PagedResult<BlockedUserResponse>>> GetBlockedUsersAsync(Guid profileId, int page, int pageSize);
}

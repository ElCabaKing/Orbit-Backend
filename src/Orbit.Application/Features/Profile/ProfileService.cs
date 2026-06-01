using Orbit.Application.Common;
using Orbit.Application.Constants;
using Orbit.Application.DTOs;
using Orbit.Application.Enums;
using Orbit.Application.Interfaces;
using Orbit.Domain.Entities;

namespace Orbit.Application.Features.Profiles;

public class ProfileService : IProfileService
{
    private readonly IGenericRepository<Orbit.Domain.Entities.Profile> _profileRepo;
    private readonly IGenericRepository<UserPrefix> _prefixRepo;
    private readonly IGenericRepository<Follow> _followRepo;
    private readonly ICloudinaryService _cloudinaryService;

    public ProfileService(
        IGenericRepository<Orbit.Domain.Entities.Profile> profileRepo,
        IGenericRepository<UserPrefix> prefixRepo,
        IGenericRepository<Follow> followRepo,
        ICloudinaryService cloudinaryService)
    {
        _profileRepo = profileRepo;
        _prefixRepo = prefixRepo;
        _followRepo = followRepo;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<ProfileResponse>> GetProfileByUsernameAsync(string username)
    {
        var slug = username.ToLowerInvariant();
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.UsernameSlug == slug);
        if (profile is null)
            return Result<ProfileResponse>.Failure(ResponseMessages.ProfileNotFound);

        var prefixResponse = await GetPrefixAsync(profile.PrefixId);
        return Result<ProfileResponse>.Success(BuildResponse(profile, prefixResponse));
    }

    public async Task<Result<ProfileResponse>> UpdateProfileAsync(Guid authUserId, string? displayName, string? bio, bool? isPrivate)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<ProfileResponse>.Failure(ResponseMessages.ProfileNotFound);

        if (displayName is not null) profile.DisplayName = displayName;
        if (bio is not null) profile.Bio = bio;
        if (isPrivate.HasValue) profile.IsPrivate = isPrivate.Value;

        profile.UpdatedAt = DateTime.UtcNow;
        _profileRepo.Update(profile);
        await _profileRepo.SaveChangesAsync();

        var prefixResponse = await GetPrefixAsync(profile.PrefixId);
        return Result<ProfileResponse>.Success(BuildResponse(profile, prefixResponse));
    }

    public async Task<Result<ProfileResponse>> UpdateProfilePictureAsync(Guid authUserId, Stream fileStream, string fileName)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<ProfileResponse>.Failure(ResponseMessages.ProfileNotFound);

        if (profile.ProfilePicturePublicId is not null)
        {
            await _cloudinaryService.DeleteAsync(profile.ProfilePicturePublicId);
        }

        var ext = Path.GetExtension(fileName);
        var uploadFileName = $"{authUserId}_{Guid.NewGuid()}{ext}";
        var uploadResult = await _cloudinaryService.UploadAsync(fileStream, uploadFileName, CloudinaryFolder.ProfilePics);

        if (!uploadResult.IsSuccess || uploadResult.Data is null)
            return Result<ProfileResponse>.Failure(ResponseMessages.FailedToUploadProfilePicture);

        profile.ProfilePictureUrl = uploadResult.Data.Url;
        profile.ProfilePicturePublicId = uploadResult.Data.PublicId;
        profile.UpdatedAt = DateTime.UtcNow;
        _profileRepo.Update(profile);
        await _profileRepo.SaveChangesAsync();

        var prefixResponse = await GetPrefixAsync(profile.PrefixId);
        return Result<ProfileResponse>.Success(BuildResponse(profile, prefixResponse));
    }

    public async Task<Result<ProfileResponse>> RemoveProfilePictureAsync(Guid authUserId)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<ProfileResponse>.Failure(ResponseMessages.ProfileNotFound);

        if (profile.ProfilePicturePublicId is not null)
        {
            await _cloudinaryService.DeleteAsync(profile.ProfilePicturePublicId);
        }

        profile.ProfilePictureUrl = null;
        profile.ProfilePicturePublicId = null;
        profile.UpdatedAt = DateTime.UtcNow;
        _profileRepo.Update(profile);
        await _profileRepo.SaveChangesAsync();

        var prefixResponse = await GetPrefixAsync(profile.PrefixId);
        return Result<ProfileResponse>.Success(BuildResponse(profile, prefixResponse));
    }

    public async Task<Result<ProfileResponse>> UpdateBannerAsync(Guid authUserId, Stream fileStream, string fileName)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<ProfileResponse>.Failure(ResponseMessages.ProfileNotFound);

        if (profile.BannerPublicId is not null)
        {
            await _cloudinaryService.DeleteAsync(profile.BannerPublicId);
        }

        var ext = Path.GetExtension(fileName);
        var uploadFileName = $"{authUserId}_{Guid.NewGuid()}{ext}";
        var uploadResult = await _cloudinaryService.UploadAsync(fileStream, uploadFileName, CloudinaryFolder.ProfileBanners);

        if (!uploadResult.IsSuccess || uploadResult.Data is null)
            return Result<ProfileResponse>.Failure(ResponseMessages.FailedToUploadBanner);

        profile.BannerUrl = uploadResult.Data.Url;
        profile.BannerPublicId = uploadResult.Data.PublicId;
        profile.UpdatedAt = DateTime.UtcNow;
        _profileRepo.Update(profile);
        await _profileRepo.SaveChangesAsync();

        var prefixResponse = await GetPrefixAsync(profile.PrefixId);
        return Result<ProfileResponse>.Success(BuildResponse(profile, prefixResponse));
    }

    public async Task<Result<ProfileResponse>> RemoveBannerAsync(Guid authUserId)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<ProfileResponse>.Failure(ResponseMessages.ProfileNotFound);

        if (profile.BannerPublicId is not null)
        {
            await _cloudinaryService.DeleteAsync(profile.BannerPublicId);
        }

        profile.BannerUrl = null;
        profile.BannerPublicId = null;
        profile.UpdatedAt = DateTime.UtcNow;
        _profileRepo.Update(profile);
        await _profileRepo.SaveChangesAsync();

        var prefixResponse = await GetPrefixAsync(profile.PrefixId);
        return Result<ProfileResponse>.Success(BuildResponse(profile, prefixResponse));
    }

    public async Task<Result<PagedResult<SearchProfileResponse>>> SearchProfilesAsync(
        string query, Guid? currentProfileId, int page, int pageSize)
    {
        var normalized = query.ToLowerInvariant();
        var skip = (page - 1) * pageSize;

        var profiles = await _profileRepo.GetPagedAsync(
            p => p.UsernameSlug.Contains(normalized) || p.DisplayName.ToLower().Contains(normalized),
            p => p.FollowersCount,
            skip,
            pageSize);

        var totalCount = await _profileRepo.CountAsync(
            p => p.UsernameSlug.Contains(normalized) || p.DisplayName.ToLower().Contains(normalized));

        HashSet<Guid> followedIds = [];
        if (currentProfileId.HasValue)
        {
            var profileIds = profiles.Select(p => p.Id).ToList();
            var existingFollows = await _followRepo.GetListAsync(f =>
                f.FollowerId == currentProfileId.Value && profileIds.Contains(f.FollowingId));
            followedIds = existingFollows.Select(f => f.FollowingId).ToHashSet();
        }

        var items = profiles.Select(p => new SearchProfileResponse(
            p.Id,
            p.Username,
            p.DisplayName,
            p.ProfilePictureUrl,
            currentProfileId.HasValue && followedIds.Contains(p.Id)
        )).ToList();

        return Result<PagedResult<SearchProfileResponse>>.Success(new PagedResult<SearchProfileResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    private async Task<UserPrefixResponse?> GetPrefixAsync(Guid? prefixId)
    {
        if (!prefixId.HasValue) return null;

        var prefix = await _prefixRepo.GetByIdAsync(prefixId.Value);
        return prefix is null ? null : new UserPrefixResponse(prefix.Id, prefix.Name, prefix.Color, prefix.IconUrl);
    }

    private static ProfileResponse BuildResponse(Orbit.Domain.Entities.Profile profile, UserPrefixResponse? prefix)
    {
        return new ProfileResponse(
            profile.Id,
            profile.Username,
            profile.DisplayName,
            profile.ProfilePictureUrl,
            profile.BannerUrl,
            profile.Bio,
            profile.FollowersCount,
            profile.FollowingCount,
            profile.IsVerified,
            prefix
        );
    }
}

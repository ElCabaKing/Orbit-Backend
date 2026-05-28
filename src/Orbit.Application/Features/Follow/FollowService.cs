using Orbit.Application.Common;
using Orbit.Application.Constants;
using Orbit.Application.DTOs;
using Orbit.Application.Interfaces;
using Orbit.Domain.Entities;

namespace Orbit.Application.Features.Follows;

public class FollowService : IFollowService
{
    private readonly IGenericRepository<Follow> _followRepo;
    private readonly IGenericRepository<Profile> _profileRepo;

    public FollowService(
        IGenericRepository<Follow> followRepo,
        IGenericRepository<Profile> profileRepo)
    {
        _followRepo = followRepo;
        _profileRepo = profileRepo;
    }

    public async Task<Result> FollowUserAsync(Guid followerProfileId, string username)
    {
        var slug = username.ToLowerInvariant();
        var targetProfile = await _profileRepo.FirstOrDefaultAsync(p => p.UsernameSlug == slug);
        if (targetProfile is null)
            return Result.Failure(ResponseMessages.ProfileNotFound);

        if (targetProfile.Id == followerProfileId)
            return Result.Failure(ResponseMessages.CannotFollowYourself);

        var existingFollow = await _followRepo.FirstOrDefaultAsync(f =>
            f.FollowerId == followerProfileId && f.FollowingId == targetProfile.Id);
        if (existingFollow is not null)
            return Result.Failure(ResponseMessages.AlreadyFollowing);

        var follow = new Follow
        {
            Id = Guid.NewGuid(),
            FollowerId = followerProfileId,
            FollowingId = targetProfile.Id,
            CreatedAt = DateTime.UtcNow,
        };

        await _followRepo.CreateAsync(follow);

        targetProfile.FollowersCount++;
        targetProfile.UpdatedAt = DateTime.UtcNow;
        _profileRepo.Update(targetProfile);
        await _profileRepo.SaveChangesAsync();

        var followerProfile = await _profileRepo.GetByIdAsync(followerProfileId);
        if (followerProfile is not null)
        {
            followerProfile.FollowingCount++;
            followerProfile.UpdatedAt = DateTime.UtcNow;
            _profileRepo.Update(followerProfile);
            await _profileRepo.SaveChangesAsync();
        }

        return Result.Success(ResponseMessages.FollowSuccessful);
    }

    public async Task<Result> UnfollowUserAsync(Guid followerProfileId, string username)
    {
        var slug = username.ToLowerInvariant();
        var targetProfile = await _profileRepo.FirstOrDefaultAsync(p => p.UsernameSlug == slug);
        if (targetProfile is null)
            return Result.Failure(ResponseMessages.ProfileNotFound);

        var follow = await _followRepo.FirstOrDefaultAsync(f =>
            f.FollowerId == followerProfileId && f.FollowingId == targetProfile.Id);
        if (follow is null)
            return Result.Failure(ResponseMessages.NotFollowing);

        await _followRepo.DeleteAsync(follow.Id);

        targetProfile.FollowersCount = Math.Max(0, targetProfile.FollowersCount - 1);
        targetProfile.UpdatedAt = DateTime.UtcNow;
        _profileRepo.Update(targetProfile);
        await _profileRepo.SaveChangesAsync();

        var followerProfile = await _profileRepo.GetByIdAsync(followerProfileId);
        if (followerProfile is not null)
        {
            followerProfile.FollowingCount = Math.Max(0, followerProfile.FollowingCount - 1);
            followerProfile.UpdatedAt = DateTime.UtcNow;
            _profileRepo.Update(followerProfile);
            await _profileRepo.SaveChangesAsync();
        }

        return Result.Success(ResponseMessages.UnfollowSuccessful);
    }

    public async Task<Result<PagedResult<PostAuthorResponse>>> GetFollowersAsync(
        string username, Guid? currentProfileId, int page, int pageSize)
    {
        var slug = username.ToLowerInvariant();
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.UsernameSlug == slug);
        if (profile is null)
            return Result<PagedResult<PostAuthorResponse>>.Failure(ResponseMessages.ProfileNotFound);

        var skip = (page - 1) * pageSize;
        var followers = await _followRepo.GetPagedAsync(
            f => f.FollowingId == profile.Id,
            f => f.CreatedAt,
            skip,
            pageSize);

        var totalCount = await _followRepo.CountAsync(f => f.FollowingId == profile.Id);

        var items = await BuildAuthorResponseList(followers, f => f.FollowerId);
        return Result<PagedResult<PostAuthorResponse>>.Success(new PagedResult<PostAuthorResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<Result<PagedResult<PostAuthorResponse>>> GetFollowingAsync(
        string username, Guid? currentProfileId, int page, int pageSize)
    {
        var slug = username.ToLowerInvariant();
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.UsernameSlug == slug);
        if (profile is null)
            return Result<PagedResult<PostAuthorResponse>>.Failure(ResponseMessages.ProfileNotFound);

        var skip = (page - 1) * pageSize;
        var following = await _followRepo.GetPagedAsync(
            f => f.FollowerId == profile.Id,
            f => f.CreatedAt,
            skip,
            pageSize);

        var totalCount = await _followRepo.CountAsync(f => f.FollowerId == profile.Id);

        var items = await BuildAuthorResponseList(following, f => f.FollowingId);
        return Result<PagedResult<PostAuthorResponse>>.Success(new PagedResult<PostAuthorResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    private async Task<List<PostAuthorResponse>> BuildAuthorResponseList(
        List<Follow> follows, Func<Follow, Guid> profileIdSelector)
    {
        var profileIds = follows.Select(profileIdSelector).Distinct().ToList();
        var profiles = new List<Profile>();
        foreach (var pid in profileIds)
        {
            var p = await _profileRepo.GetByIdAsync(pid);
            if (p is not null) profiles.Add(p);
        }
        var profileMap = profiles.ToDictionary(p => p.Id);

        return follows
            .Select(f =>
            {
                var pid = profileIdSelector(f);
                var p = profileMap.GetValueOrDefault(pid);
                return p is not null
                    ? new PostAuthorResponse(p.Id, p.Username, p.DisplayName, p.ProfilePictureUrl)
                    : new PostAuthorResponse(pid, "Unknown", "Unknown", null);
            })
            .ToList();
    }
}

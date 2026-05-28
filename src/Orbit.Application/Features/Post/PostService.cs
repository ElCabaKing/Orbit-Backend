using Orbit.Application.Common;
using Orbit.Application.Constants;
using Orbit.Application.DTOs;
using Orbit.Application.Enums;
using Orbit.Application.Interfaces;
using Orbit.Domain.Entities;

namespace Orbit.Application.Features.Posts;

public class PostService : IPostService
{
    private readonly IGenericRepository<Orbit.Domain.Entities.Post> _postRepo;
    private readonly IGenericRepository<Profile> _profileRepo;
    private readonly IGenericRepository<PostLike> _likeRepo;
    private readonly IGenericRepository<Comment> _commentRepo;
    private readonly IGenericRepository<PostMedia> _mediaRepo;
    private readonly ICloudinaryService _cloudinaryService;

    public PostService(
        IGenericRepository<Orbit.Domain.Entities.Post> postRepo,
        IGenericRepository<Profile> profileRepo,
        IGenericRepository<PostLike> likeRepo,
        IGenericRepository<Comment> commentRepo,
        IGenericRepository<PostMedia> mediaRepo,
        ICloudinaryService cloudinaryService)
    {
        _postRepo = postRepo;
        _profileRepo = profileRepo;
        _likeRepo = likeRepo;
        _commentRepo = commentRepo;
        _mediaRepo = mediaRepo;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<PostResponse>> CreatePostAsync(Guid authUserId, string content, List<MediaUploadData>? mediaFiles)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<PostResponse>.Failure(ResponseMessages.ProfileNotFound);

        var post = new Orbit.Domain.Entities.Post
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            Content = content,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _postRepo.CreateAsync(post);

        var mediaList = new List<PostMedia>();
        if (mediaFiles is not null && mediaFiles.Count > 0)
        {
            for (int i = 0; i < mediaFiles.Count; i++)
            {
                var media = mediaFiles[i];
                var ext = Path.GetExtension(media.FileName);
                var fileName = $"{profile.Id}_{Guid.NewGuid()}{ext}";
                var uploadResult = await _cloudinaryService.UploadAsync(media.FileStream, fileName, CloudinaryFolder.PostMedia);

                if (uploadResult.IsSuccess && uploadResult.Data is not null)
                {
                    var data = uploadResult.Data;
                    var postMedia = new PostMedia
                    {
                        Id = Guid.NewGuid(),
                        PostId = post.Id,
                        Url = data.Url,
                        PublicId = data.PublicId,
                        MediaType = GetMediaType(media.FileName) ?? "image",
                        Order = i,
                        Width = data.Width,
                        Height = data.Height,
                        SizeBytes = data.SizeBytes,
                        Format = data.Format,
                        DurationSeconds = data.DurationSeconds,
                        CreatedAt = DateTime.UtcNow,
                    };
                    mediaList.Add(postMedia);
                    await _mediaRepo.AddEntityAsync(postMedia);
                }
            }
        }

        profile.PostsCount++;
        profile.UpdatedAt = DateTime.UtcNow;
        _profileRepo.Update(profile);
        await _profileRepo.SaveChangesAsync();

        var author = BuildAuthorResponse(profile);
        return Result<PostResponse>.Success(BuildPostResponse(post, author, false, mediaList));
    }

    public async Task<Result<PostResponse>> GetPostAsync(Guid postId, Guid? currentProfileId)
    {
        var post = await _postRepo.FirstOrDefaultAsync(p => p.Id == postId);
        if (post is null)
            return Result<PostResponse>.Failure(ResponseMessages.PostNotFound);

        var profile = await _profileRepo.GetByIdAsync(post.ProfileId);
        if (profile is null)
            return Result<PostResponse>.Failure(ResponseMessages.PostNotFound);

        bool isLiked = false;
        if (currentProfileId.HasValue)
        {
            var like = await _likeRepo.FirstOrDefaultAsync(l => l.ProfileId == currentProfileId.Value && l.PostId == postId);
            isLiked = like is not null;
        }

        var media = await _mediaRepo.GetListAsync(m => m.PostId == postId);

        var author = BuildAuthorResponse(profile);
        return Result<PostResponse>.Success(BuildPostResponse(post, author, isLiked, media));
    }

    public async Task<Result<PagedResult<PostResponse>>> GetTimelineAsync(Guid? currentProfileId, int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        var posts = await _postRepo.GetPagedAsync(
            p => true,
            p => p.CreatedAt,
            skip,
            pageSize);

        var totalCount = await _postRepo.CountAsync(p => true);

        return await BuildPagedPostResponse(posts, totalCount, page, pageSize, currentProfileId);
    }

    public async Task<Result<PagedResult<PostResponse>>> GetProfilePostsAsync(string username, Guid? currentProfileId, int page, int pageSize)
    {
        var slug = username.ToLowerInvariant();
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.UsernameSlug == slug);
        if (profile is null)
            return Result<PagedResult<PostResponse>>.Failure(ResponseMessages.ProfileNotFound);

        var skip = (page - 1) * pageSize;
        var posts = await _postRepo.GetPagedAsync(
            p => p.ProfileId == profile.Id,
            p => p.CreatedAt,
            skip,
            pageSize);

        var totalCount = await _postRepo.CountAsync(p => p.ProfileId == profile.Id);

        return await BuildPagedPostResponse(posts, totalCount, page, pageSize, currentProfileId);
    }

    public async Task<Result<PostResponse>> UpdatePostAsync(Guid authUserId, Guid postId, string content, List<MediaUploadData>? mediaFiles = null)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result<PostResponse>.Failure(ResponseMessages.ProfileNotFound);

        var post = await _postRepo.FirstOrDefaultAsync(p => p.Id == postId && p.ProfileId == profile.Id);
        if (post is null)
            return Result<PostResponse>.Failure(ResponseMessages.PostNotFound);

        post.Content = content;
        post.UpdatedAt = DateTime.UtcNow;
        _postRepo.Update(post);

        if (mediaFiles is not null)
        {
            var existingMedia = await _mediaRepo.GetListAsync(m => m.PostId == postId);
            foreach (var m in existingMedia)
            {
                await _cloudinaryService.DeleteAsync(m.PublicId);
                _mediaRepo.Remove(m);
            }

            for (int i = 0; i < mediaFiles.Count; i++)
            {
                var media = mediaFiles[i];
                var ext = Path.GetExtension(media.FileName);
                var fileName = $"{profile.Id}_{Guid.NewGuid()}{ext}";
                var uploadResult = await _cloudinaryService.UploadAsync(media.FileStream, fileName, CloudinaryFolder.PostMedia);

                if (uploadResult.IsSuccess && uploadResult.Data is not null)
                {
                    var data = uploadResult.Data;
                    var postMedia = new PostMedia
                    {
                        Id = Guid.NewGuid(),
                        PostId = post.Id,
                        Url = data.Url,
                        PublicId = data.PublicId,
                        MediaType = GetMediaType(media.FileName) ?? "image",
                        Order = i,
                        Width = data.Width,
                        Height = data.Height,
                        SizeBytes = data.SizeBytes,
                        Format = data.Format,
                        DurationSeconds = data.DurationSeconds,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _mediaRepo.AddEntityAsync(postMedia);
                }
            }
        }

        await _postRepo.SaveChangesAsync();

        var mediaList = await _mediaRepo.GetListAsync(m => m.PostId == postId);

        var author = BuildAuthorResponse(profile);
        return Result<PostResponse>.Success(BuildPostResponse(post, author, false, mediaList));
    }

    public async Task<Result> DeletePostAsync(Guid authUserId, Guid postId)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result.Failure(ResponseMessages.ProfileNotFound);

        var post = await _postRepo.FirstOrDefaultAsync(p => p.Id == postId && p.ProfileId == profile.Id);
        if (post is null)
            return Result.Failure(ResponseMessages.PostNotFound);

        var mediaList = await _mediaRepo.GetListAsync(m => m.PostId == postId);
        foreach (var media in mediaList)
        {
            await _cloudinaryService.DeleteAsync(media.PublicId);
            _mediaRepo.Remove(media);
        }

        await _postRepo.DeleteAsync(postId);

        profile.PostsCount = Math.Max(0, profile.PostsCount - 1);
        profile.UpdatedAt = DateTime.UtcNow;
        _profileRepo.Update(profile);
        await _profileRepo.SaveChangesAsync();

        return Result.Success(ResponseMessages.PostDeleted);
    }

    public async Task<Result<LikeResponse>> LikePostAsync(Guid profileId, Guid postId)
    {
        var post = await _postRepo.FirstOrDefaultAsync(p => p.Id == postId);
        if (post is null)
            return Result<LikeResponse>.Failure(ResponseMessages.PostNotFound);

        var existingLike = await _likeRepo.FirstOrDefaultAsync(l => l.ProfileId == profileId && l.PostId == postId);
        if (existingLike is not null)
            return Result<LikeResponse>.Success(new LikeResponse(postId, true, post.LikeCount));

        var like = new PostLike
        {
            ProfileId = profileId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow,
        };

        await _likeRepo.CreateAsync(like);

        post.LikeCount++;
        post.UpdatedAt = DateTime.UtcNow;
        _postRepo.Update(post);
        await _postRepo.SaveChangesAsync();

        return Result<LikeResponse>.Success(new LikeResponse(postId, true, post.LikeCount));
    }

    public async Task<Result<LikeResponse>> UnlikePostAsync(Guid profileId, Guid postId)
    {
        var post = await _postRepo.FirstOrDefaultAsync(p => p.Id == postId);
        if (post is null)
            return Result<LikeResponse>.Failure(ResponseMessages.PostNotFound);

        var like = await _likeRepo.FirstOrDefaultAsync(l => l.ProfileId == profileId && l.PostId == postId);
        if (like is null)
            return Result<LikeResponse>.Success(new LikeResponse(postId, false, post.LikeCount));

        await _likeRepo.DeleteAsync(like.Id);

        post.LikeCount = Math.Max(0, post.LikeCount - 1);
        post.UpdatedAt = DateTime.UtcNow;
        _postRepo.Update(post);
        await _postRepo.SaveChangesAsync();

        return Result<LikeResponse>.Success(new LikeResponse(postId, false, post.LikeCount));
    }

    public async Task<Result<CommentResponse>> CreateCommentAsync(Guid profileId, Guid postId, string content)
    {
        var post = await _postRepo.FirstOrDefaultAsync(p => p.Id == postId);
        if (post is null)
            return Result<CommentResponse>.Failure(ResponseMessages.PostNotFound);

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            PostId = postId,
            Content = content,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _commentRepo.CreateAsync(comment);

        post.CommentCount++;
        post.UpdatedAt = DateTime.UtcNow;
        _postRepo.Update(post);
        await _postRepo.SaveChangesAsync();

        var profile = await _profileRepo.GetByIdAsync(profileId);
        var author = profile is not null ? BuildAuthorResponse(profile) : new PostAuthorResponse(profileId, "Unknown", "Unknown", null);

        return Result<CommentResponse>.Success(new CommentResponse(comment.Id, author, comment.Content, comment.CreatedAt));
    }

    public async Task<Result<PagedResult<CommentResponse>>> GetCommentsAsync(Guid postId, int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        var comments = await _commentRepo.GetPagedAsync(
            c => c.PostId == postId,
            c => c.CreatedAt,
            skip,
            pageSize);

        var totalCount = await _commentRepo.CountAsync(c => c.PostId == postId);

        var profileIds = comments.Select(c => c.ProfileId).Distinct().ToList();
        var profiles = new List<Profile>();
        foreach (var pid in profileIds)
        {
            var p = await _profileRepo.GetByIdAsync(pid);
            if (p is not null) profiles.Add(p);
        }
        var profileMap = profiles.ToDictionary(p => p.Id);

        var items = comments.Select(c =>
        {
            var p = profileMap.GetValueOrDefault(c.ProfileId);
            var author = p is not null ? BuildAuthorResponse(p) : new PostAuthorResponse(c.ProfileId, "Unknown", "Unknown", null);
            return new CommentResponse(c.Id, author, c.Content, c.CreatedAt);
        }).ToList();

        return Result<PagedResult<CommentResponse>>.Success(new PagedResult<CommentResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<Result> DeleteCommentAsync(Guid authUserId, Guid commentId)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.AuthUserId == authUserId);
        if (profile is null)
            return Result.Failure(ResponseMessages.ProfileNotFound);

        var comment = await _commentRepo.GetByIdAsync(commentId);
        if (comment is null)
            return Result.Failure(ResponseMessages.CommentNotFound);

        if (comment.ProfileId != profile.Id)
        {
            var post = await _postRepo.GetByIdAsync(comment.PostId);
            if (post is null || post.ProfileId != profile.Id)
                return Result.Failure(ResponseMessages.NotAuthorized);
        }

        await _commentRepo.DeleteAsync(commentId);

        var postEntity = await _postRepo.GetByIdAsync(comment.PostId);
        if (postEntity is not null)
        {
            postEntity.CommentCount = Math.Max(0, postEntity.CommentCount - 1);
            postEntity.UpdatedAt = DateTime.UtcNow;
            _postRepo.Update(postEntity);
            await _postRepo.SaveChangesAsync();
        }

        return Result.Success(ResponseMessages.CommentDeleted);
    }

    private async Task<Result<PagedResult<PostResponse>>> BuildPagedPostResponse(
        List<Orbit.Domain.Entities.Post> posts, int totalCount, int page, int pageSize, Guid? currentProfileId)
    {
        var profileIds = posts.Select(p => p.ProfileId).Distinct().ToList();
        var profiles = new List<Profile>();
        foreach (var pid in profileIds)
        {
            var p = await _profileRepo.GetByIdAsync(pid);
            if (p is not null) profiles.Add(p);
        }
        var profileMap = profiles.ToDictionary(p => p.Id);

        HashSet<Guid> likedPostIds = [];
        if (currentProfileId.HasValue && posts.Count > 0)
        {
            var postIds = posts.Select(p => p.Id).ToList();
            var likes = await _likeRepo.GetListAsync(l =>
                l.ProfileId == currentProfileId.Value && postIds.Contains(l.PostId));
            likedPostIds = likes.Select(l => l.PostId).ToHashSet();
        }

        Dictionary<Guid, List<PostMedia>> mediaMap = [];
        if (posts.Count > 0)
        {
            var postIds = posts.Select(p => p.Id).ToList();
            var allMedia = await _mediaRepo.GetListAsync(m => postIds.Contains(m.PostId));
            mediaMap = allMedia.GroupBy(m => m.PostId).ToDictionary(g => g.Key, g => g.ToList());
        }

        var items = posts.Select(p =>
        {
            var prof = profileMap.GetValueOrDefault(p.ProfileId);
            var author = prof is not null
                ? BuildAuthorResponse(prof)
                : new PostAuthorResponse(p.ProfileId, "Unknown", "Unknown", null);
            var media = mediaMap.GetValueOrDefault(p.Id) ?? [];
            return BuildPostResponse(p, author, likedPostIds.Contains(p.Id), media);
        }).ToList();

        return Result<PagedResult<PostResponse>>.Success(new PagedResult<PostResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    private static PostAuthorResponse BuildAuthorResponse(Profile profile)
    {
        return new PostAuthorResponse(
            profile.Id,
            profile.Username,
            profile.DisplayName,
            profile.ProfilePictureUrl
        );
    }

    private static PostResponse BuildPostResponse(Orbit.Domain.Entities.Post post, PostAuthorResponse author, bool isLiked, List<PostMedia> media)
    {
        return new PostResponse(
            post.Id,
            author,
            post.Content,
            media.OrderBy(m => m.Order).Select(m => new PostMediaResponse(
                m.Url,
                m.MediaType,
                m.Order,
                m.Width,
                m.Height,
                m.SizeBytes,
                m.Format,
                m.DurationSeconds
            )).ToList(),
            post.LikeCount,
            post.CommentCount,
            isLiked,
            post.CreatedAt,
            post.UpdatedAt
        );
    }

    private static string? GetMediaType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" => "image",
            ".mp4" or ".mov" or ".avi" or ".webm" => "video",
            _ => null
        };
    }
}

using Orbit.Application.DTOs;

namespace Orbit.Application.Interfaces;

public interface IHashtagService
{
    Task ProcessPostHashtags(Guid postId, string? content);
    Task<List<TrendingHashtagResponse>> GetTrendingHashtagsAsync(int count = 10, int hours = 24);
}

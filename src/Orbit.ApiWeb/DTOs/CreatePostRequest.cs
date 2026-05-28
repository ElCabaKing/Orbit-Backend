namespace Orbit.ApiWeb.DTOs;

public class CreatePostRequest
{
    public string Content { get; set; } = null!;
    public List<IFormFile>? Media { get; set; }
}

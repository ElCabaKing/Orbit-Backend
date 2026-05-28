namespace Orbit.ApiWeb.DTOs;

public class CreatePostRequest
{
    public string Content { get; set; } = null!;
    public IFormFile? Media { get; set; }
}

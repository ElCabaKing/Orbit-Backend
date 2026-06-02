namespace Orbit.ApiWeb.DTOs;

public class CreatePostRequest
{
    public string? Content { get; set; }
    public List<IFormFile>? Media { get; set; }
}

namespace Orbit.ApiWeb.DTOs;

public class UpdatePostRequest
{
    public string Content { get; set; } = null!;
    public List<IFormFile>? Media { get; set; }
}

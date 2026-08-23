namespace ZAH.Application.DTOs.Categories;

public class CategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

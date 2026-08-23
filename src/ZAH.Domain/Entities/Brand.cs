using MongoDB.Bson.Serialization.Attributes;

namespace ZAH.Domain.Entities;

[BsonIgnoreExtraElements]
public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int ProductCount { get; set; } = 0;
}

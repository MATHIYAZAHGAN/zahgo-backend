using MongoDB.Bson.Serialization.Attributes;
using ZAH.Domain.Enums;

namespace ZAH.Domain.Entities;

[BsonIgnoreExtraElements]
public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public List<Address> Addresses { get; set; } = new();
    public int RewardPoints { get; set; } = 0;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }
}

public class Address
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string? Landmark { get; set; }
    public AddressType Type { get; set; } = AddressType.Home;
    public bool IsDefault { get; set; } = false;
}

using System.ComponentModel.DataAnnotations;

namespace ZAH.Application.DTOs;

public class CreateAddressDto
{
    [Required, StringLength(100)] public string FullName { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Phone { get; set; } = string.Empty;
    [Required, StringLength(200)] public string StreetAddress { get; set; } = string.Empty;
    [Required, StringLength(80)] public string City { get; set; } = string.Empty;
    [Required, StringLength(80)] public string State { get; set; } = string.Empty;
    [Required, StringLength(10)] public string Pincode { get; set; } = string.Empty;
    public string? Landmark { get; set; }
    public string Type { get; set; } = "HOME";
    public bool IsDefault { get; set; }
}
using Azure;
using Azure.Data.Tables;

namespace FestiveGuest.Models;

public class UserEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // role
    public string RowKey { get; set; } = string.Empty; // userId
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string Status { get; set; } = "Active";
    public bool ContactEnabled { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
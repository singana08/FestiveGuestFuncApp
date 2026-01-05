namespace FestiveGuest.Models.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    public string UserId { get; set; } = string.Empty;
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
}

public class UserResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool ContactEnabled { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Token { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string Error { get; set; } = string.Empty;
}
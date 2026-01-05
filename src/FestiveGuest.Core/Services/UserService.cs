using BCrypt.Net;
using FestiveGuest.Data.Repositories;
using FestiveGuest.Models;
using FestiveGuest.Models.DTOs;
using System.Text.RegularExpressions;

namespace FestiveGuest.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public UserService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<UserResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            if (!await ValidateEmailAsync(request.Email))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Error = "Invalid email format"
                };
            }

            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Error = "Invalid credentials"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Error = "Invalid credentials"
                };
            }

            var token = await _jwtService.GenerateTokenAsync(user.RowKey, user.Email, user.Role);

            var response = MapToUserResponse(user);
            response.Token = token;

            return new ApiResponse<UserResponse>
            {
                Success = true,
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserResponse>
            {
                Success = false,
                Error = "Authentication service temporarily unavailable"
            };
        }
    }

    public async Task<ApiResponse<UserResponse>> CreateOrUpdateUserAsync(CreateUserRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Role))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Error = "UserId and Role are required"
                };
            }

            if (!string.IsNullOrEmpty(request.Email) && !await ValidateEmailAsync(request.Email))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Error = "Invalid email format"
                };
            }

            if (!string.IsNullOrEmpty(request.Password) && !await ValidatePasswordAsync(request.Password))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Error = "Password must be at least 8 characters with uppercase, lowercase, number and special character"
                };
            }

            if (!string.IsNullOrEmpty(request.Bio) && request.Bio.Length > 250)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Error = "Bio must be 250 characters or less"
                };
            }

            // Check for email conflicts
            if (!string.IsNullOrEmpty(request.Email))
            {
                var emailExists = await _userRepository.EmailExistsAsync(request.Email, request.UserId);
                if (emailExists)
                {
                    return new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Error = "Email already in use"
                    };
                }
            }

            // Get existing user to preserve createdAt
            var existingUser = await _userRepository.GetUserAsync(request.Role, request.UserId);

            var user = new UserEntity
            {
                PartitionKey = request.Role,
                RowKey = request.UserId,
                Role = request.Role,
                Name = SanitizeInput(request.Name),
                Email = request.Email?.ToLower() ?? string.Empty,
                Phone = SanitizeInput(request.Phone),
                Location = SanitizeInput(request.Location),
                Bio = SanitizeInput(request.Bio, 250),
                Lat = request.Lat,
                Lng = request.Lng,
                Status = request.Status,
                ContactEnabled = request.ContactEnabled,
                ProfileImageUrl = request.ProfileImageUrl,
                EmailVerified = request.EmailVerified,
                CreatedAt = existingUser?.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
            }
            else if (existingUser != null)
            {
                user.Password = existingUser.Password;
            }

            await _userRepository.CreateOrUpdateUserAsync(user);

            var token = await _jwtService.GenerateTokenAsync(user.RowKey, user.Email, user.Role);
            var response = MapToUserResponse(user);
            response.Token = token;

            return new ApiResponse<UserResponse>
            {
                Success = true,
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserResponse>
            {
                Success = false,
                Error = "Registration service temporarily unavailable"
            };
        }
    }

    public async Task<ApiResponse<UserResponse>> GetUserAsync(string role, string userId)
    {
        try
        {
            var user = await _userRepository.GetUserAsync(role, userId);
            if (user == null)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Error = "User not found"
                };
            }

            return new ApiResponse<UserResponse>
            {
                Success = true,
                Data = MapToUserResponse(user)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserResponse>
            {
                Success = false,
                Error = "Service temporarily unavailable"
            };
        }
    }

    public async Task<bool> ValidateEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
        return emailRegex.IsMatch(email);
    }

    public async Task<bool> ValidatePasswordAsync(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    private static string SanitizeInput(string input, int maxLength = 0)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var sanitized = input.Trim();
        if (maxLength > 0 && sanitized.Length > maxLength)
        {
            sanitized = sanitized[..maxLength];
        }

        return sanitized;
    }

    private static UserResponse MapToUserResponse(UserEntity user)
    {
        return new UserResponse
        {
            UserId = user.RowKey,
            Role = user.Role,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Location = user.Location,
            Bio = user.Bio,
            Lat = user.Lat,
            Lng = user.Lng,
            Status = user.Status,
            ContactEnabled = user.ContactEnabled,
            ProfileImageUrl = user.ProfileImageUrl,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
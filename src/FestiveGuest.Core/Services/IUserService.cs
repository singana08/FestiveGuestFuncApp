using FestiveGuest.Models;
using FestiveGuest.Models.DTOs;

namespace FestiveGuest.Core.Services;

public interface IUserService
{
    Task<ApiResponse<UserResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<UserResponse>> CreateOrUpdateUserAsync(CreateUserRequest request);
    Task<ApiResponse<UserResponse>> GetUserAsync(string role, string userId);
    Task<bool> ValidateEmailAsync(string email);
    Task<bool> ValidatePasswordAsync(string password);
}
using FestiveGuest.Models;

namespace FestiveGuest.Data.Repositories;

public interface IUserRepository
{
    Task<UserEntity?> GetUserAsync(string role, string userId);
    Task<UserEntity?> GetUserByEmailAsync(string email);
    Task<UserEntity> CreateOrUpdateUserAsync(UserEntity user);
    Task DeleteUserAsync(string role, string userId);
    Task<bool> EmailExistsAsync(string email, string excludeUserId = "");
}
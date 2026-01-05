using Azure.Data.Tables;
using FestiveGuest.Models;

namespace FestiveGuest.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TableClient _tableClient;

    public UserRepository(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<UserEntity?> GetUserAsync(string role, string userId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<UserEntity>(role, userId);
            return response.Value;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLower().Trim();
        
        await foreach (var entity in _tableClient.QueryAsync<UserEntity>(filter: $"email eq '{normalizedEmail}'"))
        {
            return entity;
        }
        
        return null;
    }

    public async Task<UserEntity> CreateOrUpdateUserAsync(UserEntity user)
    {
        await _tableClient.UpsertEntityAsync(user, TableUpdateMode.Merge);
        return user;
    }

    public async Task DeleteUserAsync(string role, string userId)
    {
        await _tableClient.DeleteEntityAsync(role, userId);
    }

    public async Task<bool> EmailExistsAsync(string email, string excludeUserId = "")
    {
        var normalizedEmail = email.ToLower().Trim();
        
        await foreach (var entity in _tableClient.QueryAsync<UserEntity>(filter: $"email eq '{normalizedEmail}'"))
        {
            if (string.IsNullOrEmpty(excludeUserId) || entity.RowKey != excludeUserId)
            {
                return true;
            }
        }
        
        return false;
    }
}
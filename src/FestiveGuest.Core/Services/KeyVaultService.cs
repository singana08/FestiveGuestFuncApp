using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;

namespace FestiveGuest.Core.Services;

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);

    public KeyVaultService(string keyVaultUrl, IMemoryCache cache)
    {
        _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        _cache = cache;
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        var cacheKey = $"secret_{secretName}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedValue) && cachedValue != null)
        {
            return cachedValue;
        }

        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            var value = secret.Value.Value;
            
            _cache.Set(cacheKey, value, _cacheExpiry);
            return value;
        }
        catch
        {
            // Fallback to environment variable
            return Environment.GetEnvironmentVariable(secretName.Replace("-", "_").ToUpper()) ?? string.Empty;
        }
    }

    public async Task<string> GetJwtSecretAsync()
    {
        return await GetSecretAsync("jwt-secret");
    }

    public async Task<string> GetStorageConnectionStringAsync()
    {
        return await GetSecretAsync("storage-connection-string");
    }
}
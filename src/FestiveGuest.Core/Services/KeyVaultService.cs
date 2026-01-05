using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FestiveGuest.Core.Services;

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<KeyVaultService> _logger;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);

    public KeyVaultService(string keyVaultUrl, IMemoryCache cache, ILogger<KeyVaultService> logger)
    {
        _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        _cache = cache;
        _logger = logger;
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
            _logger.LogDebug("Retrieving secret {SecretName} from Key Vault", secretName);
            var secret = await _secretClient.GetSecretAsync(secretName);
            var value = secret.Value.Value;
            
            if (string.IsNullOrEmpty(value))
            {
                _logger.LogWarning("Secret {SecretName} is null or empty", secretName);
                throw new InvalidOperationException($"Secret {secretName} is null or empty");
            }
            
            _cache.Set(cacheKey, value, _cacheExpiry);
            _logger.LogDebug("Successfully cached secret {SecretName}", secretName);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret {SecretName} from Key Vault", secretName);
            
            // Fallback to environment variable with consistent naming
            var envVarName = secretName.Replace("-", "_").ToUpper();
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            
            if (!string.IsNullOrEmpty(envValue))
            {
                _logger.LogWarning("Using environment variable {EnvVar} as fallback for {SecretName}", envVarName, secretName);
                _cache.Set(cacheKey, envValue, TimeSpan.FromMinutes(5)); // Shorter cache for fallback
                return envValue;
            }
            
            _logger.LogError("No fallback found for secret {SecretName}", secretName);
            throw new InvalidOperationException($"Unable to retrieve secret {secretName} from Key Vault or environment variables", ex);
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
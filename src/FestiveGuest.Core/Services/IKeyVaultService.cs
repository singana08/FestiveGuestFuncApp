namespace FestiveGuest.Core.Services;

public interface IKeyVaultService
{
    Task<string> GetSecretAsync(string secretName);
    Task<string> GetJwtSecretAsync();
    Task<string> GetStorageConnectionStringAsync();
}
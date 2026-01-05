using Azure.Data.Tables;
using FestiveGuest.Core.Services;
using FestiveGuest.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Memory;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Add memory cache
        services.AddMemoryCache();
        
        // Add Key Vault service
        var keyVaultUrl = Environment.GetEnvironmentVariable("KEY_VAULT_URL") ?? "https://kv-festive-guest.vault.azure.net/";
        services.AddSingleton<IKeyVaultService>(provider =>
        {
            var cache = provider.GetRequiredService<IMemoryCache>();
            return new KeyVaultService(keyVaultUrl, cache);
        });
        
        // Add JWT service
        services.AddSingleton<IJwtService, JwtService>();
        
        // Add Table Storage client
        services.AddSingleton<TableClient>(provider =>
        {
            var keyVaultService = provider.GetRequiredService<IKeyVaultService>();
            var connectionString = keyVaultService.GetStorageConnectionStringAsync().Result;
            return new TableClient(connectionString, "Users");
        });
        
        // Add repositories
        services.AddSingleton<IUserRepository, UserRepository>();
        
        // Add services
        services.AddSingleton<IUserService, UserService>();
    })
    .Build();

host.Run();
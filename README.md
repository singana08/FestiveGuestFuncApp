# FestiveGuest C# Azure Functions

A matrimonial platform API built with C# Azure Functions, featuring shared authentication, Table Storage, and Azure Key Vault integration.

## Architecture

- **FestiveGuest.Functions**: Azure Functions HTTP triggers
- **FestiveGuest.Core**: Business logic and services (JWT, Key Vault, User services)
- **FestiveGuest.Data**: Table Storage repositories
- **FestiveGuest.Models**: DTOs and entities

## Key Features

✅ **Shared Authentication**: Centralized JWT validation across all functions
✅ **Cached Key Vault Secrets**: Performance optimized secret retrieval
✅ **Table Storage**: Same data structure as Node.js version
✅ **Dependency Injection**: Clean service architecture
✅ **CORS Support**: Cross-origin request handling
✅ **Error Handling**: Consistent error responses

## Functions Implemented

1. **Login** - User authentication with JWT token generation
2. **CreateUpdateUser** - User registration and profile updates
3. **GetUser** - Protected endpoint with JWT validation

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools
- Azure Storage Emulator (for local development)

### Local Development

1. **Clone and restore packages**:
   ```bash
   cd D:\KalyaniMatrimony\Git\FestiveGuestFuncApp
   dotnet restore
   ```

2. **Update local.settings.json**:
   ```json
   {
     "Values": {
       "KEY_VAULT_URL": "https://your-keyvault.vault.azure.net/",
       "JWT_SECRET": "your-local-jwt-secret",
       "STORAGE_CONNECTION_STRING": "your-storage-connection"
     }
   }
   ```

3. **Run locally**:
   ```bash
   cd src/FestiveGuest.Functions
   func start
   ```

### Azure Deployment

1. **Create Function App**:
   ```bash
   az functionapp create --resource-group rg-festive-guest \
     --consumption-plan-location eastus \
     --runtime dotnet-isolated \
     --functions-version 4 \
     --name festive-guest-func-app \
     --storage-account festivegueststorage
   ```

2. **Configure App Settings**:
   ```bash
   az functionapp config appsettings set \
     --name festive-guest-func-app \
     --resource-group rg-festive-guest \
     --settings KEY_VAULT_URL=https://kv-festive-guest.vault.azure.net/
   ```

3. **Deploy**:
   ```bash
   func azure functionapp publish festive-guest-func-app
   ```

## API Endpoints

- `POST /api/Login` - User authentication
- `POST /api/CreateUpdateUser` - User registration/update
- `GET /api/GetUser` - Get user profile (requires JWT)

## Benefits Over Node.js Version

- **Shared Authentication**: No more JWT validation duplication
- **Better Performance**: Cached secrets and connections
- **Strong Typing**: Compile-time error checking
- **Easier Debugging**: Better tooling and error messages
- **Consistent Architecture**: Clean separation of concerns

## Next Steps

Add remaining functions:
- Payment processing (RecordPayment, VerifyPayment, ListPayments)
- Chat integration (IssueChatToken, GetOrCreateChatThread)
- File management (UploadImage, GetImageUrl)
- Email services (VerifyRegistrationEmail, ConfirmRegistrationEmail)
- Location services (GetLocations, SeedLocations)
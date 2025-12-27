# Local Development Guide

This guide explains how to run the Team Manager application locally with mocked authentication.

## Prerequisites

- Node.js (v18+)
- .NET 8 SDK
- Azure Functions Core Tools v4
- Azure Static Web Apps CLI (installed automatically)

## Quick Start

### 1. Install Dependencies

```bash
# Install frontend dependencies
npm install

# Install Azure Functions Core Tools (if not already installed)
npm install -g azure-functions-core-tools@4 --unsafe-perm true
```

### 2. Configure Local Settings

Create `api/local.settings.json` if it doesn't exist:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDbConnectionString": "YOUR_COSMOS_DB_CONNECTION_STRING",
    "CosmosDbDatabaseName": "TeamManager",
    "CosmosDbContainerName": "Teams"
  }
}
```

**Note:** You'll need a real Cosmos DB instance for local testing. You can:
- Use Azure Cosmos DB Emulator (Windows/Linux)
- Use a free tier Azure Cosmos DB account
- Use the existing deployed Cosmos DB connection string

### 3. Run with SWA CLI (Recommended)

The SWA CLI provides local authentication emulation:

```bash
npm run start:swa
```

This will:
- Start Angular dev server on `http://localhost:4200`
- Start Azure Functions on `http://localhost:7071`
- Start SWA CLI proxy on `http://localhost:4280`
- Mock Azure SWA authentication endpoints

**Access the app at: http://localhost:4280**

### 4. Mock Authentication

#### Option A: Using SWA CLI Built-in Auth (Easiest)

1. Navigate to `http://localhost:4280`
2. You'll see a login prompt
3. Click "Login" and the SWA CLI will simulate authentication
4. You'll be logged in as a mock user

#### Option B: Configure Mock User

Create `.auth/me.json` in your project root:

```json
{
  "clientPrincipal": {
    "userId": "test-user-123",
    "userRoles": ["authenticated"],
    "claims": [
      {
        "typ": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
        "val": "test@example.com"
      },
      {
        "typ": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
        "val": "Test User"
      }
    ],
    "identityProvider": "google",
    "userDetails": "Test User"
  }
}
```

Then run:

```bash
swa start http://localhost:4200 --api-location ./api --run "npm run start:app" --data-api-location .auth
```

### 5. Alternative: Run Services Separately

If you prefer to run services independently:

**Terminal 1 - Frontend:**
```bash
npm run start:app
```

**Terminal 2 - Backend API:**
```bash
npm run start:api
```

**Terminal 3 - SWA CLI:**
```bash
swa start http://localhost:4200 --api-location ./api
```

## Testing the Application

### 1. Create a Team

1. Login at `http://localhost:4280`
2. Navigate to `/teams`
3. Click "Create New Team"
4. Fill in team name and description
5. Submit

### 2. Generate Invite Link

1. Go to your team detail page
2. Click "Generate Invite Link"
3. Copy the link (it will be something like `http://localhost:4280/join/{token}`)

### 3. Test Join Flow

**Option 1: Using Same Browser (Different Tab)**
1. Open the invite link in a new tab
2. Enter a nickname
3. Click "Join Team"
4. You'll see "Pending Approval" status

**Option 2: Using Different Mock User**
1. Logout from the current session
2. Login as a different user (SWA CLI allows multiple mock users)
3. Open the invite link
4. Complete the join flow

### 4. Approve Pending Member

1. Go back to the team detail page as admin
2. You'll see the pending member in "Pending Approvals"
3. Click "Approve" or "Reject"

## Environment Variables

### Frontend (Angular)

No environment variables needed - Angular proxies requests through SWA CLI.

### Backend (Azure Functions)

Required in `api/local.settings.json`:

- `CosmosDbConnectionString` - Your Cosmos DB connection string
- `CosmosDbDatabaseName` - Database name (default: "TeamManager")
- `CosmosDbContainerName` - Container name (default: "Teams")

## Cosmos DB Setup

### Option 1: Azure Cosmos DB Emulator (Free, Windows/Linux)

```bash
# Install emulator
# Windows: Download from Microsoft
# Linux: Use Docker

docker run -p 8081:8081 -p 10250-10255:10250-10255 \
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
```

Connection string:
```
AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==
```

### Option 2: Azure Cosmos DB Free Tier

1. Create a free Cosmos DB account in Azure Portal
2. Create database: "TeamManager"
3. Create container: "Teams"
   - Partition key: `/id`
4. Copy connection string from Azure Portal

## Troubleshooting

### Port Conflicts

If ports are already in use:

```bash
# Change Angular port
ng serve --port 4201

# Change Functions port
func start --port 7072

# Update swa-cli.config.json accordingly
```

### Authentication Not Working

1. Make sure you're accessing `http://localhost:4280` (SWA CLI port)
2. Don't access Angular directly at `http://localhost:4200` - it won't have auth
3. Clear browser cache and try again
4. Check SWA CLI logs for authentication errors

### API Calls Failing

1. Verify Azure Functions are running: `http://localhost:7071/api/teams/my-teams`
2. Check `api/local.settings.json` has valid Cosmos DB connection
3. Ensure SWA CLI is proxying correctly
4. Check browser Network tab for 401/403 errors

### Cosmos DB Connection Issues

1. Verify connection string is correct
2. Check firewall rules (Azure Cosmos DB must allow your IP)
3. Test connection: `ping {your-cosmos-db-account}.documents.azure.com`
4. Check Cosmos DB container exists with correct partition key

## VS Code Configuration

Add to `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "SWA CLI",
      "type": "node",
      "request": "launch",
      "cwd": "${workspaceFolder}",
      "runtimeExecutable": "npm",
      "runtimeArgs": ["run", "start:swa"]
    }
  ]
}
```

## Useful Commands

```bash
# Build everything
npm run build && npm run build:api

# Run tests
npm test

# Lint/Format
npm run lint  # (if configured)

# Clean build artifacts
rm -rf dist api/bin api/obj node_modules/.cache

# View SWA CLI help
npx swa --help

# View Functions logs
# (automatically shown when running start:api)
```

## Production vs Local Differences

| Feature | Local (SWA CLI) | Production (Azure) |
|---------|----------------|-------------------|
| Auth Provider | Mock | Google OAuth |
| User ID | Mock UUID | Real Google ID |
| HTTPS | HTTP | HTTPS |
| Functions URL | localhost:7071 | Custom domain |
| Auth Endpoints | Mocked | Real Azure endpoints |

## Next Steps

- Set up Azure Cosmos DB (see above)
- Configure production deployment
- Add more mock users for testing
- Set up automated testing

## Resources

- [Azure SWA CLI Docs](https://azure.github.io/static-web-apps-cli/)
- [Azure Functions Local Development](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-local)
- [Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator)

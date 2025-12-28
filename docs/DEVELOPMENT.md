# Luminous Local Development Guide

> **Document Version:** 1.4.0
> **Last Updated:** 2025-12-28
> **Status:** Active

This guide covers setting up and running the Luminous development environment locally.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Docker Services](#docker-services)
4. [Running the API](#running-the-api)
5. [Running the Web App](#running-the-web-app)
6. [Development Authentication](#development-authentication)
7. [Email Service](#email-service-local-development)
8. [User Registration Flow](#user-registration-flow)
9. [Cosmos DB Emulator](#cosmos-db-emulator)
10. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

| Software | Version | Purpose |
|----------|---------|---------|
| **Docker Desktop** | Latest | Container runtime for local services |
| **.NET SDK** | 10.0+ | Backend API development |
| **Node.js** | 20+ | Frontend development |
| **npm** | 10+ | Package management |

### Optional Tools

| Software | Purpose |
|----------|---------|
| **Visual Studio Code** | Recommended IDE with extensions |
| **Azure Data Studio** | CosmosDB data management |
| **Redis Insight** | Redis data visualization |

### System Requirements

The Cosmos DB Emulator has specific requirements:

- **RAM**: Minimum 3GB available for Docker
- **CPU**: 2+ cores recommended
- **Disk**: 10GB free space for containers and data

### Apple Silicon (M1/M2/M3) Support

All Docker services are compatible with ARM64 architecture:

| Service | ARM64 Support | Notes |
|---------|---------------|-------|
| **Cosmos DB Emulator** | Native | First startup takes 3-5 minutes |
| **Azurite** | Native | No special configuration needed |
| **Redis** | Native | No special configuration needed |
| **Mailpit** | Native | Replaces MailHog for better ARM64 support |

**Docker Desktop Settings for Apple Silicon:**

1. Open Docker Desktop > Settings > General
2. Ensure "Use Virtualization framework" is enabled
3. Under Resources, allocate at least 4GB RAM (6GB recommended)
4. Enable "Use Rosetta for x86/amd64 emulation" for any x86 containers

**First-Time Setup:**
```bash
# Pull images (may take a few minutes)
docker compose pull

# Start services (Cosmos DB takes 3-5 minutes on first run)
docker compose up -d

# Monitor startup progress
docker compose logs -f cosmosdb
```

---

## Quick Start

### Using the Start Script

The easiest way to start development is using the provided scripts:

**macOS/Linux:**
```bash
# Start all Docker services
./scripts/dev-start.sh

# In a new terminal, start the API
./scripts/dev-start.sh --api

# In another terminal, start the web app
./scripts/dev-start.sh --web
```

**Windows (PowerShell):**
```powershell
# Start all Docker services
.\scripts\dev-start.ps1

# In a new terminal, start the API
.\scripts\dev-start.ps1 -Api

# In another terminal, start the web app
.\scripts\dev-start.ps1 -Web
```

### Manual Setup

If you prefer manual control:

```bash
# 1. Start Docker services
docker compose up -d

# 2. Start the .NET API
cd src/Luminous.Api
dotnet watch run

# 3. Start the Angular app (new terminal)
cd clients/web
npm install
npm start
```

---

## Docker Services

### Overview

The `docker-compose.yml` file provides these local services:

| Service | Port | Description |
|---------|------|-------------|
| **cosmosdb** | 8081 | Azure Cosmos DB Emulator |
| **azurite** | 10000, 10001, 10002 | Azure Storage Emulator |
| **redis** | 6379 | Redis Cache |
| **mailpit** | 1025 (SMTP), 8025 (UI) | Email testing server |

### Starting Services

```bash
# Start all services
docker compose up -d

# Start specific services
docker compose up -d cosmosdb redis

# View service status
docker compose ps

# View logs
docker compose logs -f cosmosdb

# Stop all services
docker compose down

# Stop and remove volumes (reset data)
docker compose down -v
```

### Optional Tools Profile

Additional development tools can be started with the `tools` profile:

```bash
# Start with Redis Commander web UI
docker compose --profile tools up -d
```

This adds:
- **Redis Commander**: http://localhost:8082 - Redis web UI

---

## Running the API

### Configuration

The API uses `appsettings.Development.json` for local settings:

| Setting | Value | Description |
|---------|-------|-------------|
| `CosmosDb.AccountEndpoint` | `https://localhost:8081` | Cosmos DB Emulator |
| `Redis.ConnectionString` | `localhost:6379` | Local Redis |
| `Jwt.EnableLocalTokenGeneration` | `true` | Enable dev auth |

### Starting the API

```bash
cd src/Luminous.Api

# Run with hot reload
dotnet watch run

# Or run without hot reload
dotnet run
```

### API Endpoints

Once running, the API is available at:

| Endpoint | Description |
|----------|-------------|
| http://localhost:5000 | API base URL |
| http://localhost:5000/swagger | Swagger UI (interactive docs) |
| http://localhost:5000/openapi/v1.json | OpenAPI 3.0 specification |
| http://localhost:5000/health | Health check |
| http://localhost:5000/api/devauth/status | Dev auth status |

### Launch Profiles

The API includes several launch profiles in `Properties/launchSettings.json`:

| Profile | Description |
|---------|-------------|
| `http` | HTTP only (port 5000) |
| `https` | HTTPS + HTTP (ports 5001, 5000) |
| `Luminous.Api (Watch)` | With hot reload |
| `Luminous.Api (Docker)` | Docker container |

---

## Running the Web App

### Installation

```bash
cd clients/web

# Install dependencies
npm install
```

### Starting the App

```bash
# Development server
npm start

# Or with stg configuration
npm run start:stg
```

### Web App Endpoints

| Endpoint | Description |
|----------|-------------|
| http://localhost:4200 | Angular development server |

### Build Commands

```bash
# Development build
npm run build

# Stg build
npm run build:stg

# Production build
npm run build:prod

# Run tests
npm test

# Lint code
npm run lint
```

---

## Development Authentication

### Overview

In development, you can use the local JWT token service to authenticate without setting up Azure AD or external identity providers.

### Getting a Dev Token

**Using curl:**
```bash
# Get a default dev token
curl -X POST http://localhost:5000/api/devauth/token

# Response:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "userId": "dev-user-001",
    "familyId": "dev-family-001",
    "email": "dev@luminous.local",
    "displayName": "Developer",
    "role": "Owner"
  }
}
```

**Custom token with specific user:**
```bash
curl -X POST http://localhost:5000/api/devauth/token/custom \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "test-user-001",
    "familyId": "test-family-001",
    "email": "test@luminous.local",
    "role": "Adult",
    "displayName": "Test User"
  }'
```

### Using the Token

Add the token to your requests:

```bash
curl -H "Authorization: Bearer <your-token>" \
  http://localhost:5000/api/families
```

### Swagger UI

The Swagger UI includes JWT authentication support:

1. Open http://localhost:5000/swagger
2. Click "Authorize"
3. Enter your token (without "Bearer " prefix)
4. Click "Authorize"

### Token Configuration

Token settings in `appsettings.Development.json`:

```json
{
  "Jwt": {
    "SecretKey": "LuminousDevSecretKey-AtLeast32Characters-ForHMACSHA256!",
    "Issuer": "https://luminous.local",
    "Audience": "luminous-api",
    "ExpirationMinutes": 60,
    "EnableLocalTokenGeneration": true
  }
}
```

> **Security Note**: Local token generation is only available when `EnableLocalTokenGeneration` is `true` AND the application is running in the Development environment.

---

## Email Service (Local Development)

### Overview

In local development, emails are **logged to the console** instead of being sent. This allows you to test authentication flows (OTP, invitations) without configuring a real email service.

### Configuration

The email service is controlled by `Email__UseDevelopmentMode` in `appsettings.Development.json`:

```json
{
  "Email": {
    "UseDevelopmentMode": true,
    "ConnectionString": "",
    "SenderAddress": "noreply@luminous.local",
    "SenderName": "Luminous",
    "BaseUrl": "http://localhost:4200",
    "HelpUrl": "http://localhost:4200/help"
  }
}
```

### Viewing Email Output

When `UseDevelopmentMode` is `true`, all emails are logged to the console:

```
📧 [DEV EMAIL] OTP Code for user@example.com: 123456
```

Look for log entries with the `📧 [DEV EMAIL]` prefix when testing:
- OTP authentication
- Family invitations
- Welcome emails

### Testing OTP Authentication

```bash
# Request an OTP code
curl -X POST http://localhost:5000/api/auth/otp/request \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com"}'

# Watch the console output for the OTP code, then verify it:
curl -X POST http://localhost:5000/api/auth/otp/verify \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "code": "123456"}'
```

### Email Templates

Email templates are located in `src/Luminous.Api/Templates/Email/`:

| Template | Purpose |
|----------|---------|
| `base.hbs` | Base HTML layout (header, footer, styling) |
| `otp.hbs` | One-time password email content |
| `invitation.hbs` | Family invitation email content |
| `welcome.hbs` | Welcome email for new members |

Templates use [Handlebars](https://handlebarsjs.com/) syntax for dynamic content.

### Switching to Real Email (Optional)

If you need to test with real email delivery locally:

1. Set up an Azure Communication Services resource
2. Update `appsettings.Development.json`:

```json
{
  "Email": {
    "UseDevelopmentMode": false,
    "ConnectionString": "endpoint=https://your-acs.communication.azure.com/;accesskey=...",
    "SenderAddress": "DoNotReply@your-domain.azurecomm.net",
    "SenderName": "Luminous",
    "BaseUrl": "http://localhost:4200",
    "HelpUrl": "http://localhost:4200/help"
  }
}
```

---

## User Registration Flow

### Overview

Luminous uses a secure 2-step registration flow with email verification. This prevents attackers from registering accounts using someone else's email address.

### Registration Steps

```
1. POST /api/auth/register/start
   → Validates email, stores in session, sends OTP

2. POST /api/auth/otp/verify
   → Verifies OTP code, marks email as verified

3. POST /api/auth/register/complete
   → Creates user with session email, registers passkey
```

### Testing Registration Locally

**Step 1: Start Registration**
```bash
curl -X POST http://localhost:5000/api/auth/register/start \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "displayName": "New User"
  }'

# Response includes sessionId
# Watch console for OTP code (in development mode)
```

**Step 2: Verify Email with OTP**
```bash
# Use the OTP code from the console output
curl -X POST http://localhost:5000/api/auth/otp/verify \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "code": "123456"
  }'
```

**Step 3: Complete Registration with Passkey**

This step requires a WebAuthn-compatible browser or client. The Angular app handles this automatically.

### Joining an Existing Family

To join an existing family, include the invite code in Step 1:

```bash
curl -X POST http://localhost:5000/api/auth/register/start \
  -H "Content-Type: application/json" \
  -d '{
    "email": "familymember@example.com",
    "displayName": "Family Member",
    "inviteCode": "ABC123"
  }'
```

### Security Notes

- Email is stored **server-side in session**, not trusted from the client
- Registration fails if email is not verified via OTP
- The `/api/auth/register/complete` endpoint does NOT accept email as a parameter
- Invite codes are validated against existing family invitations

---

## Cosmos DB Emulator

### Overview

The Azure Cosmos DB Emulator provides a local environment for development. It supports the SQL API used by Luminous.

### Connection Details

| Property | Value |
|----------|-------|
| **Endpoint** | `https://localhost:8081` |
| **Account Key** | `C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==` |
| **Database** | `luminous` |

### Data Explorer

Access the Cosmos DB Data Explorer at: https://localhost:8081/_explorer/index.html

> **Note**: You may need to accept the self-signed certificate warning in your browser.

### SSL Certificate Setup

The emulator uses a self-signed certificate. To avoid SSL errors:

**macOS:**
```bash
# Download and trust the certificate
./scripts/dev-start.sh --install-cert

# Or manually:
curl -k https://localhost:8081/_explorer/emulator.pem > ~/cosmos-emulator.crt
sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain ~/cosmos-emulator.crt
```

**Windows (PowerShell):**
```powershell
.\scripts\dev-start.ps1 -InstallCert

# Or manually:
Invoke-WebRequest -Uri "https://localhost:8081/_explorer/emulator.pem" -SkipCertificateCheck -OutFile cosmos-emulator.crt
Import-Certificate -FilePath .\cosmos-emulator.crt -CertStoreLocation Cert:\CurrentUser\Root
```

**Linux:**
```bash
curl -k https://localhost:8081/_explorer/emulator.pem > cosmos-emulator.crt
sudo cp cosmos-emulator.crt /usr/local/share/ca-certificates/
sudo update-ca-certificates
```

### Startup Time

The Cosmos DB Emulator can take **2-3 minutes** to start initially. The dev scripts wait for it automatically.

### Containers

The emulator will create these containers (defined in Bicep):

| Container | Partition Key | Description |
|-----------|--------------|-------------|
| `families` | `/id` | Family (tenant) data |
| `users` | `/familyId` | User accounts |
| `events` | `/familyId` | Calendar events |
| `chores` | `/familyId` | Chores and tasks |
| `devices` | `/familyId` | Linked devices |
| `routines` | `/familyId` | Daily routines |
| `lists` | `/familyId` | Shopping/custom lists |
| `meals` | `/familyId` | Meal plans |
| `completions` | `/familyId` | Task completions |
| `invitations` | `/familyId` | Member invitations |
| `credentials` | `/userId` | WebAuthn credentials |

---

## Troubleshooting

### Docker Issues

**Cosmos DB Emulator won't start:**
```bash
# Check if you have enough memory
docker stats

# Increase Docker memory to at least 3GB
# Docker Desktop > Settings > Resources > Memory

# Reset the emulator data
docker compose down -v
docker compose up -d cosmosdb
```

**Port already in use:**
```bash
# Find what's using the port
lsof -i :5000  # macOS/Linux
netstat -ano | findstr :5000  # Windows

# Kill the process or use a different port
```

### API Issues

**API can't connect to Cosmos DB:**
1. Ensure Docker services are running: `docker compose ps`
2. Wait for Cosmos DB to be ready (2-3 minutes on first start)
3. Check the SSL certificate is trusted
4. Verify `appsettings.Development.json` has correct settings

**JWT token errors:**
1. Ensure `EnableLocalTokenGeneration` is `true`
2. Check you're running in Development environment
3. Verify the token hasn't expired (default 60 minutes)

### Web App Issues

**npm install fails:**
```bash
# Clear npm cache
npm cache clean --force

# Delete node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
```

**CORS errors:**
1. Ensure the API is running
2. Check `Cors.AllowedOrigins` includes `http://localhost:4200`
3. Verify you're using `http://localhost:4200` not `127.0.0.1`

### Apple Silicon (ARM64) Issues

**Cosmos DB Emulator won't start or is very slow:**
```bash
# Check Docker has enough resources (need 4GB+ RAM)
docker stats

# Try pulling the latest image explicitly
docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

# Reset and try again
docker compose down -v
docker compose up -d cosmosdb

# Watch the logs (startup can take 3-5 minutes)
docker compose logs -f cosmosdb
```

**"no matching manifest for linux/arm64" error:**
```bash
# Some images may need x86_64 emulation via Rosetta 2
# Ensure Rosetta is enabled in Docker Desktop settings

# For specific services, you can force the platform:
# Edit docker-compose.yml and add:
#   platform: linux/amd64
```

**Slow performance on Apple Silicon:**
1. Ensure Docker Desktop has "Use Virtualization framework" enabled
2. Allocate at least 4GB RAM (6GB recommended) in Docker settings
3. Enable "Use Rosetta for x86/amd64 emulation" for better x86 compatibility
4. Consider using native ARM64 images where available (all Luminous services support ARM64)

### Reset Everything

To completely reset the development environment:

```bash
# Stop and remove all containers and volumes
docker compose down -v

# Remove all Luminous data
docker volume rm luminous-cosmosdb-data luminous-azurite-data luminous-redis-data

# Clean build artifacts
dotnet clean
rm -rf src/*/bin src/*/obj
rm -rf clients/web/node_modules clients/web/dist

# Start fresh
./scripts/dev-start.sh
```

---

## VS Code Setup

The repository includes complete VS Code configuration in the `.vscode/` folder.

### Installing Recommended Extensions

When you open the project, VS Code will prompt you to install recommended extensions. Alternatively:

1. Open Command Palette (`Cmd+Shift+P` / `Ctrl+Shift+P`)
2. Run "Extensions: Show Recommended Extensions"
3. Click "Install All" in the Workspace Recommendations section

### Key Extensions

| Extension | Purpose |
|-----------|---------|
| **C# Dev Kit** | C# language support, debugging, testing |
| **Angular Language Service** | Angular template intellisense |
| **Tailwind CSS IntelliSense** | Tailwind class autocomplete |
| **Docker** | Container management |
| **Bicep** | Azure infrastructure files |
| **REST Client** | API testing from `.http` files |
| **GitLens** | Enhanced Git integration |

### Tasks (Ctrl+Shift+B)

Access via Command Palette > "Tasks: Run Task":

| Task | Description |
|------|-------------|
| **Docker: Start All Services** | Start CosmosDB, Redis, Azurite, Mailpit |
| **Docker: Stop All Services** | Stop all Docker containers |
| **Solution: Build All** | Build entire .NET solution |
| **Solution: Test All** | Run all tests |
| **API: Watch (Hot Reload)** | Start API with hot reload |
| **Web: Start Dev Server** | Start Angular dev server |
| **Dev: Start Full Stack** | Start Docker + API + Web |
| **Dev: Get Auth Token** | Get a development JWT token |

### Debugging (F5)

Pre-configured launch configurations:

| Configuration | Description |
|---------------|-------------|
| **API: Debug** | Debug .NET API (opens Swagger) |
| **API: Debug (HTTPS)** | Debug with HTTPS enabled |
| **Web: Debug in Chrome** | Debug Angular app in Chrome |
| **Web: Debug in Edge** | Debug Angular app in Edge |
| **Full Stack: API + Web (Chrome)** | Debug both API and Web together |
| **Tests: Debug All** | Debug all tests |

### Compound Debugging

For full-stack debugging:

1. Select "Full Stack: API + Web (Chrome)" from the debug dropdown
2. Press F5
3. Both API and Web will start with debuggers attached
4. Set breakpoints in both C# and TypeScript code

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+B` | Run default build task |
| `F5` | Start debugging |
| `Ctrl+Shift+D` | Open Debug panel |
| `Ctrl+Shift+P` | Command Palette |
| `Ctrl+`` ` | Toggle terminal |

### REST Client Usage

Create `.http` files to test API endpoints:

```http
### Get Dev Token
POST http://localhost:5000/api/devauth/token

### Get Family (with token)
GET http://localhost:5000/api/families
Authorization: Bearer {{token}}
```

Use the REST Client extension to send requests directly from VS Code.

### Project Structure in Explorer

The settings hide build artifacts (`bin/`, `obj/`, `node_modules/`) for cleaner navigation. To show hidden files, adjust `files.exclude` in settings.

---

## Related Documentation

- [Project Overview](./PROJECT-OVERVIEW.md)
- [Architecture](./ARCHITECTURE.md)
- [Roadmap](./ROADMAP.md)
- [Azure Infrastructure](./AZURE-INFRASTRUCTURE.md)
- [CLAUDE.md (Development Guidelines)](../CLAUDE.md)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-22 | Luminous Team | Initial development guide |
| 1.1.0 | 2025-12-22 | Luminous Team | Added comprehensive VS Code configuration |
| 1.2.0 | 2025-12-22 | Luminous Team | Added ARM64/Apple Silicon support and Mailpit |
| 1.3.0 | 2025-12-27 | Luminous Team | Added Email Service documentation (local development) |
| 1.4.0 | 2025-12-28 | Luminous Team | Added User Registration Flow, OpenAPI endpoint |

# Luminous Local Development Guide

> **Document Version:** 1.0.0
> **Last Updated:** 2025-12-22
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
7. [Cosmos DB Emulator](#cosmos-db-emulator)
8. [Troubleshooting](#troubleshooting)

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
| **mailhog** | 1025 (SMTP), 8025 (UI) | Email testing server |

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
| http://localhost:5000/swagger | Swagger UI |
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

# Or with staging configuration
npm run start:staging
```

### Web App Endpoints

| Endpoint | Description |
|----------|-------------|
| http://localhost:4200 | Angular development server |

### Build Commands

```bash
# Development build
npm run build

# Staging build
npm run build:staging

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

## IDE Configuration

### Visual Studio Code

Recommended extensions:
- C# Dev Kit
- Angular Language Service
- Tailwind CSS IntelliSense
- Docker
- REST Client

### VS Code Tasks

Create `.vscode/tasks.json`:
```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "Start Docker Services",
      "type": "shell",
      "command": "docker compose up -d",
      "problemMatcher": []
    },
    {
      "label": "Start API",
      "type": "shell",
      "command": "dotnet watch run",
      "options": {
        "cwd": "${workspaceFolder}/src/Luminous.Api"
      },
      "isBackground": true
    },
    {
      "label": "Start Web",
      "type": "shell",
      "command": "npm start",
      "options": {
        "cwd": "${workspaceFolder}/clients/web"
      },
      "isBackground": true
    }
  ]
}
```

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

# Azure Infrastructure Guide

> **Document Version:** 1.0.0
> **Last Updated:** 2025-12-27
> **Status:** Active
> **TOGAF Phase:** Phase D (Technology Architecture)

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture Diagram](#architecture-diagram)
3. [Resource Inventory](#resource-inventory)
4. [Prerequisites](#prerequisites)
5. [Deployment Guide](#deployment-guide)
6. [Environment Configuration](#environment-configuration)
7. [Security Configuration](#security-configuration)
8. [Cost Estimation](#cost-estimation)
9. [Monitoring and Observability](#monitoring-and-observability)
10. [Troubleshooting](#troubleshooting)
11. [Related Documents](#related-documents)

---

## Overview

Luminous uses Azure as its cloud platform, following the **TP-1: Azure-Native Stack** architecture principle. The infrastructure is defined as code using Bicep with Azure Verified Modules (AVMs) for consistency and best practices.

### Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Bicep over ARM/Terraform** | Native Azure support, simpler syntax, better tooling |
| **Azure Verified Modules** | Pre-validated, secure, and compliant patterns |
| **Multi-tenant via partitioning** | Cost-effective, simpler operations |
| **Managed services** | Reduced operational overhead |

### Infrastructure Principles

Following TOGAF TP-4 (Infrastructure as Code):

- **Reproducible**: Same code deploys identical infrastructure
- **Version controlled**: All changes tracked in Git
- **Environment parity**: Dev/stg/prd use same templates
- **Self-documenting**: Bicep templates serve as documentation

---

## Architecture Diagram

```
                                    ┌─────────────────────────────────────────┐
                                    │         AZURE SUBSCRIPTION              │
                                    └─────────────────────────────────────────┘
                                                       │
                                    ┌─────────────────────────────────────────┐
                                    │     RESOURCE GROUP: rg-lum-{env}        │
                                    └─────────────────────────────────────────┘
                                                       │
        ┌──────────────────────────────────────────────┼──────────────────────────────────────────────┐
        │                                              │                                              │
        ▼                                              ▼                                              ▼
┌───────────────┐                             ┌───────────────┐                             ┌───────────────┐
│    CLIENTS    │                             │   COMPUTE     │                             │     DATA      │
├───────────────┤                             ├───────────────┤                             ├───────────────┤
│               │         HTTPS               │               │                             │               │
│  Static Web   │◄────────────────────────────│  App Service  │                             │  Cosmos DB    │
│  App (SWA)    │                             │  (.NET API)   │◄────────────────────────────│  (Core Data)  │
│               │                             │               │                             │               │
│  stapp-lum-*  │                             │  app-lum-*    │                             │  cosmos-lum-* │
└───────────────┘                             └───────┬───────┘                             └───────────────┘
        ▲                                             │                                              ▲
        │                                             │ SignalR                                      │
        │                                     ┌───────┴───────┐                                      │
        │                                     │               │                                      │
        │                                     │   SignalR     │                                      │
        │                                     │   Service     │                                      │
        │                                     │               │                                      │
        │                                     │  sigr-lum-*   │                                      │
        │                                     └───────────────┘                                      │
        │                                                                                            │
        │                                     ┌───────────────┐                                      │
        │                                     │               │                                      │
        │                                     │  Function App │                                      │
        │                                     │  (Sync Jobs)  │──────────────────────────────────────┤
        │                                     │               │                                      │
        │                                     │ func-lum-sync │                                      │
        │                                     └───────────────┘                                      │
        │                                                                                            │
        │                                     ┌───────────────┐                             ┌───────────────┐
        │                                     │               │                             │               │
        │                                     │  Function App │                             │  Blob Storage │
        │                                     │  (Import)     │────────────────────────────▶│  (Media)      │
        │                                     │               │                             │               │
        │                                     │func-lum-import│                             │  stlum*       │
        │                                     └───────────────┘                             └───────────────┘
        │                                             │
        │                                             │ Service Bus
        │                                     ┌───────┴───────┐
        │                                     │               │
        │                                     │  Service Bus  │
        │                                     │  (Messaging)  │
        │                                     │               │
        │                                     │  sb-lum-*     │
        │                                     └───────────────┘
        │
        │                                     ┌───────────────────────────────────────────────────────┐
        │                                     │                      SECURITY                         │
        │                                     ├───────────────┬───────────────┬───────────────────────┤
        │                                     │   Key Vault   │ App Config    │     Redis Cache       │
        │                                     │  (Secrets)    │ (Settings)    │    (Sessions)         │
        │                                     │  kv-lum-*     │ appcs-lum-*   │   redis-lum-*         │
        │                                     └───────────────┴───────────────┴───────────────────────┘
        │
        │                                     ┌───────────────────────────────────────────────────────┐
        │                                     │                     MONITORING                        │
        │                                     ├───────────────────────────────┬───────────────────────┤
        │                                     │      Log Analytics            │  Application Insights │
        │                                     │      log-lum-*                │     appi-lum-*        │
        │                                     └───────────────────────────────┴───────────────────────┘
        │
        │
┌───────┴───────┐  ┌───────────────┐  ┌───────────────┐
│   iOS App     │  │  Android App  │  │  Display App  │
│   (Swift)     │  │   (Kotlin)    │  │  (Electron)   │
└───────────────┘  └───────────────┘  └───────────────┘
```

---

## Resource Inventory

### Resource Naming Convention

Resources use a naming pattern with a **6-character unique suffix** derived from `uniqueString(resourceGroup().id)` to ensure global uniqueness while remaining readable. The suffix is deterministic - the same resource group always produces the same suffix.

**Pattern**: `{type}-lum-{env}-{suffix}` (e.g., `app-lum-dev-a1b2c3`)

### Compute Resources

| Resource | Name Pattern | Purpose | SKU (Dev/Prod) |
|----------|--------------|---------|----------------|
| App Service Plan | `asp-lum-{env}` | Hosts API and Functions | B1 / P1v3 |
| App Service | `app-lum-{env}-{suffix}` | .NET 10 REST API | - |
| Function App | `func-lum-{env}-{suffix}-sync` | Calendar sync jobs | - |
| Function App | `func-lum-{env}-{suffix}-import` | Import processing | - |
| Static Web App | `stapp-lum-{env}-{suffix}` | Angular web app | Free / Standard |

### Data Resources

| Resource | Name Pattern | Purpose | SKU (Dev/Prod) |
|----------|--------------|---------|----------------|
| Cosmos DB | `cosmos-lum-{env}-{suffix}` | Document database | Serverless / Provisioned |
| Storage Account | `stlum{env}{suffix}` | Blob storage | Standard_LRS / Standard_GRS |
| Redis Cache | `redis-lum-{env}-{suffix}` | Session cache | Basic C0 / Standard C1 |

### Messaging Resources

| Resource | Name Pattern | Purpose | SKU (Dev/Prod) |
|----------|--------------|---------|----------------|
| Service Bus | `sb-lum-{env}-{suffix}` | Async messaging | Basic / Standard |
| SignalR Service | `sigr-lum-{env}-{suffix}` | Real-time sync | Free / Standard |

### Communication Resources

| Resource | Name Pattern | Purpose |
|----------|--------------|---------|
| Email Service | `email-lum-{env}-{suffix}` | Email delivery service |
| Communication Service | `acs-lum-{env}-{suffix}` | Azure Communication Services |

### Security Resources

| Resource | Name Pattern | Purpose |
|----------|--------------|---------|
| Key Vault | `kv-lum-{env}-{suffix}` | Secrets management |
| App Configuration | `appcs-lum-{env}-{suffix}` | Centralized config |

### Monitoring Resources (RG-scoped, no suffix needed)

| Resource | Name Pattern | Purpose |
|----------|--------------|---------|
| Log Analytics | `log-lum-{env}` | Centralized logging |
| Application Insights | `appi-lum-{env}` | APM and diagnostics |

---

## Prerequisites

### Required Tools

| Tool | Version | Purpose |
|------|---------|---------|
| Azure CLI | 2.50+ | Azure resource management |
| Bicep CLI | 0.22+ | Infrastructure as Code |
| Git | 2.30+ | Version control |

### Installation

```bash
# Install Azure CLI (macOS)
brew install azure-cli

# Install Azure CLI (Windows)
winget install Microsoft.AzureCLI

# Install Bicep CLI
az bicep install

# Verify installations
az --version
az bicep version
```

### Azure Access Requirements

1. **Azure Subscription** with Owner or Contributor role
2. **Resource Groups** must be pre-created before deployment:
   - `rg-lum-dev` - Development environment
   - `rg-lum-stg` - Staging environment
   - `rg-lum-prd` - Production environment
3. **Service Principal** for CI/CD with Contributor role on the resource groups (not subscription-wide)
4. **Registered Resource Providers**:
   - Microsoft.DocumentDB
   - Microsoft.Web
   - Microsoft.Storage
   - Microsoft.Cache
   - Microsoft.SignalRService
   - Microsoft.ServiceBus
   - Microsoft.KeyVault
   - Microsoft.AppConfiguration
   - Microsoft.Insights
   - Microsoft.OperationalInsights

```bash
# Register required providers
az provider register --namespace Microsoft.DocumentDB
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.Storage
az provider register --namespace Microsoft.Cache
az provider register --namespace Microsoft.SignalRService
az provider register --namespace Microsoft.ServiceBus
az provider register --namespace Microsoft.KeyVault
az provider register --namespace Microsoft.AppConfiguration
az provider register --namespace Microsoft.Insights
az provider register --namespace Microsoft.OperationalInsights
```

---

## Deployment Guide

### Directory Structure

```
infra/
├── bicep/
│   ├── main.bicep              # Main orchestration (uses AVMs from registry)
│   └── parameters/             # Environment configs
│       ├── dev.bicepparam
│       ├── stg.bicepparam
│       └── prd.bicepparam
└── scripts/
    ├── deploy.sh               # Bash deployment script
    └── deploy.ps1              # PowerShell deployment script
```

### Azure Verified Modules (AVMs)

All resources are deployed using AVMs directly from the public Bicep registry (`br/public:avm/res/...`). This approach provides:

- **Pre-validated patterns** - Security and compliance built-in
- **Maintained by Microsoft/community** - Regular updates and bug fixes
- **Semantic versioning** - Predictable upgrades
- **Less code to maintain** - No custom module implementations

| Resource | AVM Reference |
|----------|---------------|
| Log Analytics | `br/public:avm/res/operational-insights/workspace:0.14.2` |
| App Insights | `br/public:avm/res/insights/component:0.7.1` |
| Key Vault | `br/public:avm/res/key-vault/vault:0.13.3` |
| App Configuration | `br/public:avm/res/app-configuration/configuration-store:0.9.2` |
| Cosmos DB | `br/public:avm/res/document-db/database-account:0.18.0` |
| Storage Account | `br/public:avm/res/storage/storage-account:0.31.0` |
| Redis Cache | `br/public:avm/res/cache/redis:0.16.4` |
| Service Bus | `br/public:avm/res/service-bus/namespace:0.16.0` |
| SignalR | `br/public:avm/res/signal-r-service/signal-r:0.10.1` |
| App Service Plan | `br/public:avm/res/web/serverfarm:0.5.0` |
| App Service/Functions | `br/public:avm/res/web/site:0.19.4` |
| Static Web App | `br/public:avm/res/web/static-site:0.9.3` |
| Email Service | `br/public:avm/res/communication/email-service:0.3.2` |
| Communication Service | `br/public:avm/res/communication/communication-service:0.4.2` |

### Quick Start

```bash
# 1. Login to Azure
az login

# 2. Select subscription
az account set --subscription "<subscription-name-or-id>"

# 3. Create resource groups (one-time setup)
az group create --name rg-lum-dev --location eastus2
az group create --name rg-lum-stg --location eastus2
az group create --name rg-lum-prd --location eastus2

# 4. Deploy to development
cd infra/scripts
./deploy.sh dev

# 5. Deploy to stg
./deploy.sh stg

# 6. Deploy to production (with what-if preview first)
./deploy.sh prd --what-if
./deploy.sh prd
```

### Detailed Deployment Steps

#### Step 1: Authenticate

```bash
# Interactive login
az login

# Or service principal login (CI/CD)
az login --service-principal \
  --username $ARM_CLIENT_ID \
  --password $ARM_CLIENT_SECRET \
  --tenant $ARM_TENANT_ID
```

#### Step 2: Create Resource Group (if not exists)

```bash
# Create resource group for target environment
az group create --name rg-lum-dev --location eastus2
```

#### Step 3: Validate Templates

```bash
# Build and validate Bicep
az bicep build --file infra/bicep/main.bicep

# What-if analysis (resource group scope)
az deployment group what-if \
  --resource-group rg-lum-dev \
  --template-file infra/bicep/main.bicep \
  --parameters @infra/bicep/parameters/dev.bicepparam
```

#### Step 4: Deploy

```bash
# Deploy infrastructure (resource group scope)
az deployment group create \
  --name "luminous-$(date +%Y%m%d)" \
  --resource-group rg-lum-dev \
  --template-file infra/bicep/main.bicep \
  --parameters @infra/bicep/parameters/dev.bicepparam
```

#### Step 5: Verify

```bash
# List deployment outputs
az deployment group show \
  --name "luminous-$(date +%Y%m%d)" \
  --resource-group rg-lum-dev \
  --query properties.outputs

# Test API endpoint
curl https://app-lum-dev-api.azurewebsites.net/health
```

---

## Post-Deployment Configuration

After deploying the infrastructure, some resources require manual configuration.

### Azure Communication Services (Email)

The infrastructure deployment creates the Email Service with an Azure-managed domain and links it to a Communication Service. However, you must manually configure the connection string and sender address in the App Service.

#### Step 1: Wait for Domain Provisioning

The Azure-managed email domain takes a few minutes to provision. Check the status:

1. Open the Azure Portal
2. Navigate to **Email Communication Services** > `email-lum-{env}-{suffix}`
3. Go to **Provision domains** in the left menu
4. Wait for the Azure-managed domain to show **Status: Ready**

The domain will look like: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net`

#### Step 2: Get the Sender Address

Once the domain is provisioned:

1. In the Email Service, go to **Provision domains**
2. Click on the **Azure-managed domain**
3. Go to **MailFrom addresses**
4. Copy the sender address (e.g., `DoNotReply@xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net`)

#### Step 3: Get the ACS Connection String

1. Navigate to **Communication Services** > `acs-lum-{env}-{suffix}`
2. Go to **Settings** > **Keys**
3. Copy the **Primary connection string**

#### Step 4: Store Connection String in Key Vault

For security, store the connection string in Key Vault:

```bash
# Store the ACS connection string
az keyvault secret set \
  --vault-name kv-lum-{env}-{suffix} \
  --name acs-connection-string \
  --value "endpoint=https://acs-lum-{env}-{suffix}.communication.azure.com/;accesskey=..."
```

#### Step 5: Configure App Service Settings

Add the email configuration to the App Service:

```bash
# Using Key Vault reference for connection string (recommended)
az webapp config appsettings set \
  --resource-group rg-lum-{env} \
  --name app-lum-{env}-{suffix} \
  --settings \
    "Email__ConnectionString=@Microsoft.KeyVault(VaultName=kv-lum-{env}-{suffix};SecretName=acs-connection-string)" \
    "Email__SenderAddress=DoNotReply@xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net"
```

Or via the Azure Portal:

1. Navigate to **App Service** > `app-lum-{env}-{suffix}`
2. Go to **Settings** > **Configuration**
3. Add the following Application Settings:

| Name | Value |
|------|-------|
| `Email__ConnectionString` | `@Microsoft.KeyVault(VaultName=kv-lum-...;SecretName=acs-connection-string)` |
| `Email__SenderAddress` | `DoNotReply@xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net` |

4. Click **Save** and restart the App Service

#### Step 6: Verify Email Sending

Test the email configuration:

```bash
# Get a dev token (if using dev environment)
curl -X POST https://app-lum-dev-{suffix}.azurewebsites.net/api/devauth/token

# Request an OTP (this will send a real email in Azure)
curl -X POST https://app-lum-dev-{suffix}.azurewebsites.net/api/auth/otp/request \
  -H "Content-Type: application/json" \
  -d '{"email": "your-email@example.com"}'
```

Check the Application Insights logs for email delivery status.

#### Troubleshooting Email

| Issue | Solution |
|-------|----------|
| "Connection string is not configured" | Ensure `Email__ConnectionString` is set and App Service has Key Vault access |
| "Sender address not valid" | Verify the domain is provisioned and address format matches |
| "403 Forbidden" | Check the App Service managed identity has access to Key Vault secrets |
| "Domain not found" | Wait for domain provisioning (can take 5-10 minutes) |

#### Email Configuration Summary

| Setting | Description | Example |
|---------|-------------|---------|
| `Email__UseDevelopmentMode` | `false` for Azure, `true` for local | Set by Bicep deployment |
| `Email__ConnectionString` | ACS connection string | Store in Key Vault |
| `Email__SenderAddress` | Verified sender address | `DoNotReply@{guid}.azurecomm.net` |
| `Email__SenderName` | Display name for emails | `Luminous` |
| `Email__BaseUrl` | Base URL for email links | `https://{static-web-app-url}` |
| `Email__HelpUrl` | Help center URL | `https://{static-web-app-url}/help` |

---

## Environment Configuration

### Development

- **Purpose**: Local development and testing
- **Cost optimization**: Serverless Cosmos DB, Basic SKUs
- **Features**: Full feature set, relaxed security

```
Cosmos DB:     Serverless
App Service:   B1 (Basic)
Redis:         Basic C0
SignalR:       Free
Static Web:    Free
```

### Staging

- **Purpose**: Pre-production testing
- **Configuration**: Production-like with lower capacity
- **Features**: Full feature set, production security

```
Cosmos DB:     Provisioned (autoscale 400-4000 RU/s)
App Service:   S1 (Standard)
Redis:         Standard C0
SignalR:       Standard S1
Static Web:    Standard
```

### Production

- **Purpose**: Live user traffic
- **Configuration**: High availability, redundancy
- **Features**: Full feature set, strict security

```
Cosmos DB:     Provisioned (autoscale 400-4000 RU/s)
App Service:   P1v3 (Premium)
Redis:         Standard C1
SignalR:       Standard S1
Static Web:    Standard
```

---

## Security Configuration

### Key Vault Integration

All secrets are stored in Azure Key Vault and referenced via managed identity:

```csharp
// appsettings.json references
{
  "CosmosDb": {
    "ConnectionString": "@Microsoft.KeyVault(VaultName=kv-lum-dev;SecretName=cosmosdb-connection)"
  }
}
```

### Managed Identity

All compute resources use system-assigned managed identity:

1. **App Service** → Key Vault (secrets)
2. **App Service** → Cosmos DB (data access)
3. **Function Apps** → Key Vault, Cosmos DB, Storage

### Network Security

| Resource | Public Access | Notes |
|----------|---------------|-------|
| App Service | Enabled | HTTPS only |
| Cosmos DB | Enabled | Firewall rules recommended |
| Storage | Enabled | Private endpoints optional |
| Key Vault | Enabled | RBAC authorization |

### RBAC Roles

| Principal | Resource | Role |
|-----------|----------|------|
| App Service MI | Key Vault | Key Vault Secrets User |
| App Service MI | Cosmos DB | Cosmos DB Data Contributor |
| App Service MI | Storage | Storage Blob Data Contributor |
| Function App MI | Service Bus | Azure Service Bus Data Sender/Receiver |

---

## Cost Estimation

### Development Environment (Monthly)

| Resource | SKU | Estimated Cost |
|----------|-----|----------------|
| Cosmos DB | Serverless | ~$5-20 |
| App Service | B1 | ~$13 |
| Redis | Basic C0 | ~$16 |
| SignalR | Free | $0 |
| Static Web App | Free | $0 |
| Storage | Standard LRS | ~$1 |
| Service Bus | Basic | ~$0.05 |
| Key Vault | Standard | ~$0.03 |
| App Config | Free | $0 |
| Log Analytics | Pay-as-you-go | ~$2 |
| **Total** | | **~$40-60/month** |

### Production Environment (Monthly)

| Resource | SKU | Estimated Cost |
|----------|-----|----------------|
| Cosmos DB | 400-4000 RU/s | ~$25-100 |
| App Service | P1v3 | ~$130 |
| Redis | Standard C1 | ~$80 |
| SignalR | Standard S1 | ~$50 |
| Static Web App | Standard | ~$9 |
| Storage | Standard GRS | ~$5 |
| Service Bus | Standard | ~$10 |
| Key Vault | Standard | ~$0.03 |
| App Config | Standard | ~$1 |
| Log Analytics | Pay-as-you-go | ~$10 |
| **Total** | | **~$320-400/month** |

*Note: Costs are estimates and may vary based on usage.*

---

## Monitoring and Observability

### Application Insights

Configured for all compute resources:

- **Distributed tracing** across API and Functions
- **Live metrics** for real-time monitoring
- **Availability tests** for endpoint monitoring
- **Smart detection** for anomaly alerts

### Log Analytics Queries

```kusto
// API request latency (95th percentile)
requests
| where timestamp > ago(1h)
| summarize percentile(duration, 95) by bin(timestamp, 5m)
| render timechart

// Error rate by operation
requests
| where timestamp > ago(24h)
| summarize total = count(), errors = countif(success == false) by operation_Name
| extend errorRate = errors * 100.0 / total
| order by errorRate desc

// Cosmos DB RU consumption
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.DOCUMENTDB"
| where Category == "DataPlaneRequests"
| summarize sum(todouble(requestCharge_s)) by bin(TimeGenerated, 5m)
| render timechart
```

### Alerts (Recommended)

| Alert | Condition | Severity |
|-------|-----------|----------|
| API Error Rate | > 5% errors in 5 min | Critical |
| Response Time | P95 > 2 seconds | Warning |
| Cosmos DB Throttling | 429 errors > 10 in 5 min | Warning |
| Function Failures | Any failure | Warning |

---

## Troubleshooting

### Common Issues

#### Deployment Fails - Resource Provider Not Registered

```
Error: The subscription is not registered to use namespace 'Microsoft.DocumentDB'
```

**Solution:**
```bash
az provider register --namespace Microsoft.DocumentDB
az provider wait --namespace Microsoft.DocumentDB --timeout 300
```

#### Cosmos DB Connection Issues

```
Error: Unable to connect to Cosmos DB
```

**Solution:**
1. Verify managed identity has `Cosmos DB Data Contributor` role
2. Check firewall allows Azure services
3. Verify connection string format

#### App Service Health Check Fails

```
Error: Health check endpoint returning 500
```

**Solution:**
1. Check Application Insights for exceptions
2. Verify app settings are configured
3. Check Key Vault access

### Logs and Diagnostics

```bash
# View App Service logs
az webapp log tail --name app-lum-dev-api --resource-group rg-lum-dev

# View Function App logs
az functionapp logs show --name func-lum-dev-sync --resource-group rg-lum-dev

# Query Application Insights
az monitor app-insights query \
  --app appi-lum-dev \
  --analytics-query "requests | take 10"
```

---

## Related Documents

- [Project Overview](./PROJECT-OVERVIEW.md)
- [Architecture Overview](./ARCHITECTURE.md)
- [Development Roadmap](./ROADMAP.md)
- [ADR-003: Azure as Cloud Platform](./adr/ADR-003-azure-cloud-platform.md)
- [ADR-007: Bicep with AVMs for IaC](./adr/ADR-007-bicep-avm-iac.md)
- [Local Development Guide](./DEVELOPMENT.md) (coming soon)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-21 | Luminous Team | Initial infrastructure documentation |
| 1.1.0 | 2025-12-27 | Luminous Team | Updated AVM module versions to latest |
| 1.2.0 | 2025-12-27 | Luminous Team | Added Azure Communication Services (Email) and post-deployment configuration |

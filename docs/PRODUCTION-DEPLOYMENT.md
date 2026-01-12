# Luminous Production Deployment Guide

> **Document Version:** 1.0.0
> **Last Updated:** 2026-01-12
> **Status:** Active

This guide covers deploying Luminous to production with a custom domain, including DNS configuration, SSL certificates, and release management.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Infrastructure Deployment](#infrastructure-deployment)
3. [Custom Domain Setup](#custom-domain-setup)
4. [Application Deployment](#application-deployment)
5. [Post-Deployment Verification](#post-deployment-verification)
6. [Display App Release](#display-app-release)
7. [OSS Customization](#oss-customization)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Azure Requirements

1. **Azure Subscription** with sufficient quota for:
   - App Service Plan (P1v3)
   - Cosmos DB (provisioned throughput)
   - Azure Static Web Apps (Standard)
   - Azure DNS Zone
   - Other supporting services (Redis, SignalR, etc.)

2. **Azure CLI** installed and authenticated:
   ```bash
   az login
   az account set --subscription "<subscription-id>"
   ```

3. **Resource Group** created for production:
   ```bash
   az group create --name rg-lum-prd --location eastus2
   ```

4. **Service Principal** for GitHub Actions with required roles:
   - Contributor on the resource group
   - User Access Administrator (for role assignments)

### Domain Requirements

1. **Domain Name** registered with a domain registrar (e.g., Namecheap, GoDaddy, Cloudflare)
2. **Access** to modify nameserver (NS) records at your registrar

### GitHub Secrets

Configure these secrets in your GitHub repository (Settings > Secrets and variables > Actions):

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Service principal client ID |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `AZURE_STATIC_WEB_APP_TOKEN_PRD` | Static Web App deployment token (generated after infra deployment) |

---

## Infrastructure Deployment

### Step 1: Configure Production Parameters

Edit `infra/bicep/parameters/prd.bicepparam` to set your custom domain:

```bicep
// Custom Domain Configuration
param customDomain = 'luminousfamily.com'  // Replace with your domain
param deployDnsZone = true
```

### Step 2: Deploy Infrastructure

**Option A: Using Deployment Script**

```bash
# Navigate to infra directory
cd infra

# Deploy to production
./scripts/deploy.sh prd

# Or preview changes first
./scripts/deploy.sh prd --what-if
```

**Option B: Using Azure CLI**

```bash
az deployment group create \
  --resource-group rg-lum-prd \
  --template-file infra/bicep/main.bicep \
  --parameters @infra/bicep/parameters/prd.bicepparam
```

**Option C: Using GitHub Actions**

1. Go to Actions > Infrastructure Deployment
2. Click "Run workflow"
3. Select `prd` environment
4. Wait for deployment to complete

### Step 3: Capture Deployment Outputs

After deployment, note these important outputs:

```bash
# Get deployment outputs
az deployment group show \
  --resource-group rg-lum-prd \
  --name <deployment-name> \
  --query properties.outputs
```

Key outputs to note:
- `dnsZoneNameServers` - Azure DNS nameservers (needed for domain delegation)
- `staticWebAppUrl` - Default hostname (for testing)
- `webAppUrl` - Production URL with custom domain
- `appServiceUrl` - API endpoint

---

## Custom Domain Setup

### Step 1: Configure Domain Nameservers

After infrastructure deployment, Azure creates a DNS zone. You must delegate your domain to Azure DNS by updating your registrar's nameserver records.

1. **Get Azure DNS Nameservers** from deployment output:
   ```
   ns1-XX.azure-dns.com
   ns2-XX.azure-dns.net
   ns3-XX.azure-dns.org
   ns4-XX.azure-dns.info
   ```

2. **Update Nameservers at Your Registrar:**
   - Log in to your domain registrar
   - Navigate to DNS/Nameserver settings
   - Replace existing nameservers with Azure DNS nameservers
   - Save changes

3. **Wait for DNS Propagation** (up to 48 hours, typically 15-60 minutes)

### Step 2: Verify DNS Propagation

```bash
# Check nameserver delegation
nslookup -type=NS luminousfamily.com

# Should return Azure DNS nameservers
```

### Step 3: Verify Custom Domain Binding

The infrastructure deployment automatically:
- Creates the DNS zone
- Configures A record (alias) for apex domain
- Configures CNAME record for www subdomain
- Binds the custom domain to Static Web App
- Provisions SSL certificate (automatic, may take up to 24 hours)

Check domain status in Azure Portal:
1. Navigate to Static Web App > Custom domains
2. Verify domain shows "Ready" status
3. SSL certificate shows "Certificate issued"

### Step 4: Test Domain Access

```bash
# Test apex domain
curl -I https://luminousfamily.com

# Test www subdomain
curl -I https://www.luminousfamily.com

# Both should return HTTP 200 with valid SSL
```

---

## Application Deployment

### Step 1: Get Static Web App Deployment Token

```bash
# Get the deployment token for GitHub Actions
az staticwebapp secrets list \
  --name stapp-lum-prd-<suffix> \
  --resource-group rg-lum-prd \
  --query properties.apiKey -o tsv
```

Save this as `AZURE_STATIC_WEB_APP_TOKEN_PRD` in GitHub Secrets.

### Step 2: Deploy Application via GitHub Actions

**Manual Deployment:**
1. Go to Actions > Deploy Application
2. Click "Run workflow"
3. Select `prd` environment
4. Enable both API and Web deployment
5. Wait for deployment to complete

**Automatic Deployment:**
- Push to `main` branch triggers dev deployment
- Production requires manual workflow dispatch

### Step 3: Verify Deployment

1. **Check Static Web App:** https://luminousfamily.com
2. **Check API Health:** https://app-lum-prd-<suffix>.azurewebsites.net/health
3. **Test Authentication:** Try passkey registration and login

---

## Post-Deployment Verification

### Checklist

- [ ] **Web App** accessible at custom domain
- [ ] **SSL Certificate** valid and auto-renewed
- [ ] **API Health** endpoint returns 200
- [ ] **Passkey Registration** works with custom domain
- [ ] **Email Links** point to custom domain
- [ ] **Calendar OAuth** redirects to custom domain
- [ ] **SignalR** real-time sync working

### Configure OAuth Providers

Update OAuth redirect URIs in provider consoles:

**Google Cloud Console:**
1. Navigate to APIs & Services > Credentials
2. Edit your OAuth 2.0 Client
3. Add authorized redirect URI: `https://luminousfamily.com/auth/calendar/callback`

**Azure AD App Registration:**
1. Navigate to App registrations > Your app
2. Add redirect URI: `https://luminousfamily.com/auth/calendar/callback`

### Update Key Vault Secrets

After first deployment, update placeholder secrets with real credentials:

```bash
# Update Google Calendar OAuth credentials
az keyvault secret set \
  --vault-name kv-lum-prd-<suffix> \
  --name calendar-google-client-id \
  --value "<your-google-client-id>"

az keyvault secret set \
  --vault-name kv-lum-prd-<suffix> \
  --name calendar-google-client-secret \
  --value "<your-google-client-secret>"

# Update Microsoft Calendar OAuth credentials
az keyvault secret set \
  --vault-name kv-lum-prd-<suffix> \
  --name calendar-microsoft-client-id \
  --value "<your-microsoft-client-id>"

az keyvault secret set \
  --vault-name kv-lum-prd-<suffix> \
  --name calendar-microsoft-client-secret \
  --value "<your-microsoft-client-secret>"
```

---

## Display App Release

### Creating a Production Release

The Luminous Display App is an Electron application for wall-mounted displays.

#### Step 1: Build for Production

```bash
cd clients/display

# Install dependencies
npm ci

# Build for all platforms
npm run electron:build

# Or build for specific platform
npm run electron:build:linux
npm run electron:build:windows
npm run electron:build:mac
```

#### Step 2: Locate Built Artifacts

After build, find packages in:
```
clients/display/release/
├── Luminous-Display-1.0.0.AppImage      # Linux
├── Luminous-Display-1.0.0.deb           # Debian/Ubuntu
├── Luminous-Display-Setup-1.0.0.exe     # Windows
└── Luminous-Display-1.0.0.dmg           # macOS
```

#### Step 3: Create GitHub Release

1. **Tag the Release:**
   ```bash
   git tag -a v1.0.0 -m "Release v1.0.0"
   git push origin v1.0.0
   ```

2. **Create Release on GitHub:**
   - Go to Releases > Draft a new release
   - Select the tag
   - Title: `Luminous Display v1.0.0`
   - Add release notes describing changes
   - Upload built artifacts (.AppImage, .exe, .dmg, .deb)
   - Mark as pre-release if testing, or publish directly

3. **Update Documentation:**
   - Update version in `clients/display/package.json`
   - Update `clients/display/CHANGELOG.md`

#### Step 4: Configure Auto-Update (Optional)

For production auto-updates, configure electron-updater:

1. Set `publish` configuration in `clients/display/package.json`:
   ```json
   {
     "build": {
       "publish": {
         "provider": "github",
         "owner": "trickpatty",
         "repo": "Luminous"
       }
     }
   }
   ```

2. Ensure releases are published (not draft) for auto-update to work.

### Display Device Setup

See `clients/display/README.md` for complete setup instructions including:
- Kiosk mode installation
- Auto-start configuration
- Device linking

---

## OSS Customization

Luminous is open-source and designed for customization. Here's how to deploy with your own domain:

### Using Your Own Domain

1. **Fork the Repository** on GitHub

2. **Update Production Parameters:**
   ```bicep
   // infra/bicep/parameters/prd.bicepparam
   param customDomain = 'yourfamilyhub.com'  // Your domain
   param deployDnsZone = true
   ```

3. **Update Angular Environments:**
   ```typescript
   // clients/web/src/environments/environment.prod.ts
   export const environment = {
     production: true,
     apiUrl: 'https://yourfamilyhub.com/api',
     webAuthn: {
       rpId: 'yourfamilyhub.com',
       rpName: 'Your Family Hub',
       origin: 'https://yourfamilyhub.com'
     }
   };
   ```

4. **Update Display App Configuration:**
   ```typescript
   // clients/display/src/environments/environment.prod.ts
   export const environment = {
     production: true,
     apiUrl: 'https://yourfamilyhub.com/api',
     signalRUrl: 'https://app-xxx.azurewebsites.net/hubs/sync'
   };
   ```

5. **Deploy Infrastructure and Application** following the steps above.

### Customization Options

| What | Where | Notes |
|------|-------|-------|
| **Domain** | `prd.bicepparam` | Custom domain parameter |
| **Branding** | `clients/*/src/assets/` | Replace logos and icons |
| **Colors** | `design-tokens/tokens.json` | Update brand colors |
| **App Name** | Multiple files | Search for "Luminous" and replace |
| **Email Sender** | `main.bicep` | Update sender display name |

---

## Troubleshooting

### DNS Issues

**Domain not resolving:**
```bash
# Check NS records
dig NS luminousfamily.com

# Check A record
dig A luminousfamily.com

# Check propagation status
# Use: https://www.whatsmydns.net/
```

**SSL Certificate not issued:**
1. Verify DNS propagation is complete
2. Check Azure Portal > Static Web App > Custom domains
3. Wait up to 24 hours for certificate provisioning
4. Re-add custom domain if stuck in "Pending" state

### Deployment Failures

**Infrastructure deployment fails:**
```bash
# Check deployment status
az deployment group show \
  --resource-group rg-lum-prd \
  --name <deployment-name> \
  --query properties.error
```

**Application deployment fails:**
1. Check GitHub Actions logs for specific errors
2. Verify Static Web App deployment token is valid
3. Ensure Azure CLI is logged in with correct subscription

### Authentication Issues

**Passkeys not working on custom domain:**
1. Verify `Fido2__ServerDomain` matches custom domain
2. Verify `Fido2__Origins__0` includes custom domain URL
3. Clear browser passkey cache and re-register

**Email links pointing to wrong domain:**
1. Verify `Email__BaseUrl` is set correctly in App Service settings
2. Redeploy infrastructure to update settings

### Real-time Sync Issues

**SignalR not connecting:**
1. Check browser console for WebSocket errors
2. Verify SignalR endpoint is accessible
3. Check CORS configuration includes custom domain

---

## Related Documents

- [Architecture Overview](./ARCHITECTURE.md)
- [Azure Infrastructure](./AZURE-INFRASTRUCTURE.md)
- [CI/CD Pipeline](./CI-CD.md)
- [Display App README](../clients/display/README.md)
- [Development Guide](./DEVELOPMENT.md)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-01-12 | Luminous Team | Initial production deployment guide |

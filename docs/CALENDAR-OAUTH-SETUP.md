# Calendar OAuth Setup Guide

> **Document Version:** 1.0.0
> **Last Updated:** 2026-01-04
> **Status:** Active

This guide covers setting up OAuth credentials for Google Calendar and Microsoft/Outlook calendar integrations.

---

## Table of Contents

1. [Overview](#overview)
2. [Google Calendar Setup](#google-calendar-setup)
3. [Microsoft Outlook Setup](#microsoft-outlook-setup)
4. [Local Development Configuration](#local-development-configuration)
5. [Azure Production Configuration](#azure-production-configuration)
6. [Testing the Integration](#testing-the-integration)
7. [Troubleshooting](#troubleshooting)

---

## Overview

Luminous supports connecting to external calendars via OAuth 2.0:

| Provider | Calendar Types | Required Credentials |
|----------|---------------|---------------------|
| **Google** | Google Calendar | Client ID, Client Secret |
| **Microsoft** | Outlook, Microsoft 365 | Client ID, Client Secret, Tenant ID |

Both integrations require you to register an OAuth application with the respective provider and configure the credentials in Luminous.

---

## Google Calendar Setup

### Step 1: Create a Google Cloud Project

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Click **Select a project** → **New Project**
3. Enter a project name (e.g., "Luminous Calendar Integration")
4. Click **Create**

### Step 2: Enable the Google Calendar API

1. In the Cloud Console, go to **APIs & Services** → **Library**
2. Search for "Google Calendar API"
3. Click on **Google Calendar API**
4. Click **Enable**

### Step 3: Configure OAuth Consent Screen

1. Go to **APIs & Services** → **OAuth consent screen**
2. Select **External** user type (unless you have a Google Workspace org)
3. Click **Create**
4. Fill in the required fields:
   - **App name**: Luminous Family Hub
   - **User support email**: Your email address
   - **Developer contact email**: Your email address
5. Click **Save and Continue**
6. Add the following scopes:
   - `https://www.googleapis.com/auth/calendar.readonly`
   - `https://www.googleapis.com/auth/calendar.events.readonly`
   - `https://www.googleapis.com/auth/userinfo.email`
7. Click **Save and Continue**
8. Add test users (your email) during development
9. Click **Save and Continue**

### Step 4: Create OAuth Credentials

1. Go to **APIs & Services** → **Credentials**
2. Click **Create Credentials** → **OAuth client ID**
3. Select **Web application** as the application type
4. Enter a name (e.g., "Luminous Web Client")
5. Add **Authorized redirect URIs**:
   - For local development: `http://localhost:4200/auth/calendar/callback`
   - For production: `https://your-app-domain.azurestaticapps.net/auth/calendar/callback`
6. Click **Create**
7. Copy the **Client ID** and **Client Secret**

### Step 5: Publishing (Production)

For production use with any Google account:

1. Go to **OAuth consent screen**
2. Click **Publish App**
3. Complete Google's verification process (may require privacy policy, terms of service)

> **Note:** While in "Testing" mode, only test users you've added can authorize the app.

---

## Microsoft Outlook Setup

### Step 1: Register an Application in Azure AD

1. Go to the [Azure Portal](https://portal.azure.com/)
2. Navigate to **Microsoft Entra ID** (formerly Azure AD)
3. Go to **App registrations** → **New registration**
4. Fill in the registration form:
   - **Name**: Luminous Calendar Integration
   - **Supported account types**: "Accounts in any organizational directory and personal Microsoft accounts"
   - **Redirect URI**: Select "Web" and enter:
     - For local development: `http://localhost:4200/auth/calendar/callback`
5. Click **Register**
6. Copy the **Application (client) ID** - this is your Client ID

### Step 2: Create a Client Secret

1. In your app registration, go to **Certificates & secrets**
2. Click **New client secret**
3. Enter a description (e.g., "Luminous Production")
4. Select an expiration period (24 months recommended)
5. Click **Add**
6. **Copy the secret value immediately** - it won't be shown again

> **Important:** Store this secret securely. You cannot view it again after leaving this page.

### Step 3: Configure API Permissions

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Select **Delegated permissions**
5. Add the following permissions:
   - `Calendars.Read` - Read user calendars
   - `User.Read` - Sign in and read user profile
   - `offline_access` - Maintain access to data (for refresh tokens)
6. Click **Add permissions**

> **Note:** These permissions do not require admin consent for personal Microsoft accounts.

### Step 4: Add Additional Redirect URIs (Production)

1. Go to **Authentication**
2. Under **Web** → **Redirect URIs**, add your production URL:
   - `https://your-app-domain.azurestaticapps.net/auth/calendar/callback`
3. Click **Save**

---

## Local Development Configuration

### Option 1: appsettings.Development.json

Edit `src/Luminous.Api/appsettings.Development.json`:

```json
{
  "Calendar": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Microsoft": {
      "ClientId": "YOUR_MICROSOFT_CLIENT_ID",
      "ClientSecret": "YOUR_MICROSOFT_CLIENT_SECRET",
      "TenantId": "common"
    },
    "DefaultRedirectUri": "http://localhost:4200/auth/calendar/callback"
  }
}
```

### Option 2: Environment Variables

Set environment variables (useful for CI/CD or secrets management):

```bash
# Google Calendar
export Calendar__Google__ClientId="YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
export Calendar__Google__ClientSecret="YOUR_GOOGLE_CLIENT_SECRET"

# Microsoft/Outlook
export Calendar__Microsoft__ClientId="YOUR_MICROSOFT_CLIENT_ID"
export Calendar__Microsoft__ClientSecret="YOUR_MICROSOFT_CLIENT_SECRET"
export Calendar__Microsoft__TenantId="common"

# Redirect URI
export Calendar__DefaultRedirectUri="http://localhost:4200/auth/calendar/callback"
```

### Option 3: User Secrets (Recommended for Development)

Use .NET User Secrets to keep credentials out of source control:

```bash
cd src/Luminous.Api

# Google credentials
dotnet user-secrets set "Calendar:Google:ClientId" "YOUR_GOOGLE_CLIENT_ID"
dotnet user-secrets set "Calendar:Google:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET"

# Microsoft credentials
dotnet user-secrets set "Calendar:Microsoft:ClientId" "YOUR_MICROSOFT_CLIENT_ID"
dotnet user-secrets set "Calendar:Microsoft:ClientSecret" "YOUR_MICROSOFT_CLIENT_SECRET"
```

---

## Azure Production Configuration

### Key Vault Secrets

After deploying the Azure infrastructure, update the Key Vault secrets:

1. Go to the [Azure Portal](https://portal.azure.com/)
2. Navigate to your Key Vault (named `kv-luminous-{env}`)
3. Go to **Secrets**
4. Update each placeholder secret with actual values:

| Secret Name | Value |
|-------------|-------|
| `calendar-google-client-id` | Your Google OAuth Client ID |
| `calendar-google-client-secret` | Your Google OAuth Client Secret |
| `calendar-microsoft-client-id` | Your Microsoft App Client ID |
| `calendar-microsoft-client-secret` | Your Microsoft App Client Secret |

### Using Azure CLI

```bash
# Set the Key Vault name
KV_NAME="kv-luminous-prd"  # or kv-luminous-dev

# Update Google credentials
az keyvault secret set --vault-name $KV_NAME \
  --name "calendar-google-client-id" \
  --value "YOUR_GOOGLE_CLIENT_ID"

az keyvault secret set --vault-name $KV_NAME \
  --name "calendar-google-client-secret" \
  --value "YOUR_GOOGLE_CLIENT_SECRET"

# Update Microsoft credentials
az keyvault secret set --vault-name $KV_NAME \
  --name "calendar-microsoft-client-id" \
  --value "YOUR_MICROSOFT_CLIENT_ID"

az keyvault secret set --vault-name $KV_NAME \
  --name "calendar-microsoft-client-secret" \
  --value "YOUR_MICROSOFT_CLIENT_SECRET"
```

### Restart the App Service

After updating secrets, restart the App Service to pick up new values:

```bash
az webapp restart --name "app-luminous-prd" --resource-group "rg-luminous-prd"
```

---

## Testing the Integration

### 1. Start the API

```bash
cd src/Luminous.Api
dotnet run
```

### 2. Test OAuth Flow via API

Use the calendar connection endpoints to test:

```bash
# Start OAuth flow for Google
curl -X POST "http://localhost:5000/api/calendar-connections/family/{familyId}/oauth/start" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -d '{"provider": "Google", "redirectUri": "http://localhost:4200/auth/calendar/callback"}'
```

### 3. Verify Configuration

Check that credentials are loaded correctly in the API logs:

```
info: Luminous.Infrastructure.Calendar[0]
      Calendar providers initialized: Google (configured: True), Microsoft (configured: True)
```

---

## Troubleshooting

### "Missing required parameter: client_id"

**Cause:** The `Calendar` configuration section is missing or the `ClientId` is empty.

**Solution:**
1. Verify `appsettings.Development.json` has the `Calendar` section
2. Check that `ClientId` values are not empty strings
3. Restart the API after configuration changes

### "redirect_uri_mismatch" (Google)

**Cause:** The redirect URI in the OAuth request doesn't match the configured URIs in Google Cloud Console.

**Solution:**
1. Check the exact redirect URI being used
2. Add it to **Authorized redirect URIs** in Google Cloud Console
3. URIs must match exactly, including trailing slashes and protocol

### "AADSTS50011: The reply URL specified in the request does not match" (Microsoft)

**Cause:** Similar to Google - redirect URI mismatch.

**Solution:**
1. Go to your app registration in Azure Portal
2. Add the exact redirect URI under **Authentication** → **Redirect URIs**
3. Click **Save**

### "Access blocked: Authorization Error" (Google)

**Cause:** App is in testing mode and user is not a test user.

**Solution:**
1. Add the user's email to test users in OAuth consent screen, OR
2. Publish the app for production use

### OAuth Session Expired

**Cause:** OAuth sessions expire after 15 minutes.

**Solution:** Start a new OAuth flow. The session-based flow is designed for security.

### Tokens Not Refreshing

**Cause:** Refresh token may be expired or revoked.

**Solution:**
1. For Google: Users must re-authorize if refresh token is revoked
2. For Microsoft: Check if the app secret has expired in Azure AD

---

## Security Best Practices

1. **Never commit credentials** to source control
2. **Use User Secrets** for local development
3. **Use Key Vault** for production credentials
4. **Rotate client secrets** periodically (Microsoft secrets expire)
5. **Monitor OAuth consent** in provider dashboards for suspicious activity
6. **Use minimal scopes** - only request read access to calendars

---

## Related Documentation

- [Local Development Guide](./DEVELOPMENT.md)
- [Azure Infrastructure](./AZURE-INFRASTRUCTURE.md)
- [ADR-003: Azure Cloud Platform](./adr/ADR-003-azure-cloud-platform.md)

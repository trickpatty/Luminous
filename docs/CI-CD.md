# Luminous CI/CD Documentation

> **Document Version:** 1.0.0
> **Last Updated:** 2025-12-22
> **Status:** Active
> **TOGAF Phase:** Phase G (Implementation Governance)

---

## Table of Contents

1. [Overview](#overview)
2. [GitHub Actions Workflows](#github-actions-workflows)
3. [Environment Configuration](#environment-configuration)
4. [Secrets Management](#secrets-management)
5. [Deployment Process](#deployment-process)
6. [Dependabot Configuration](#dependabot-configuration)
7. [Troubleshooting](#troubleshooting)

---

## Overview

Luminous uses GitHub Actions for continuous integration and deployment. The CI/CD pipeline follows TOGAF principles and ensures code quality, security, and reliable deployments.

### Pipeline Architecture

```
+------------------+     +------------------+     +------------------+
|  Push/PR Event   |---->|   Build & Test   |---->|   Deploy (Auto)  |
+------------------+     +------------------+     +------------------+
                                |                         |
                                v                         v
                        +---------------+         +---------------+
                        | .NET Tests    |         | Dev           |
                        | Angular Tests |         | Staging       |
                        | Bicep Lint    |         | Production    |
                        +---------------+         +---------------+
```

### Key Principles

| Principle | Implementation |
|-----------|----------------|
| **Shift Left** | Run tests and security scans on every PR |
| **Environment Parity** | Same artifacts deployed to all environments |
| **Infrastructure as Code** | Bicep templates for all Azure resources |
| **Automated Updates** | Dependabot for dependency management |

---

## GitHub Actions Workflows

### 1. .NET Build and Test (`dotnet.yml`)

**Triggers:**
- Push to `main` or `develop` branches (when `.NET` files change)
- Pull requests targeting `main` or `develop`
- Manual workflow dispatch

**Jobs:**

| Job | Description | Runs On |
|-----|-------------|---------|
| `build` | Restore, build, test, and collect coverage | Every trigger |
| `security-scan` | Check for vulnerable NuGet packages | Pull requests only |

**Artifacts:**
- `dotnet-build`: Compiled binaries (7-day retention)
- `security-scan-results`: Vulnerability report (30-day retention)

**Example Usage:**
```bash
# Trigger manually via GitHub CLI
gh workflow run dotnet.yml
```

### 2. Angular Build and Test (`angular.yml`)

**Triggers:**
- Push to `main` or `develop` branches (when `clients/web/**` changes)
- Pull requests targeting `main` or `develop`
- Manual workflow dispatch

**Jobs:**

| Job | Description | Runs On |
|-----|-------------|---------|
| `build` | Lint, typecheck, test, and build production | Every trigger |
| `build-stg` | Build stg configuration | Develop branch and PRs |
| `security-audit` | Run npm audit for vulnerabilities | Pull requests only |
| `bundle-analysis` | Analyze bundle size | Pull requests only |

**Artifacts:**
- `angular-build-production`: Production build (7-day retention)
- `angular-build-stg`: Stg build (7-day retention)
- `npm-audit-results`: Security audit report (30-day retention)

### 3. Infrastructure Deployment (`infrastructure.yml`)

**Triggers:**
- Push to `main` or `develop` branches (when `infra/**` changes)
- Pull requests targeting `main` or `develop`
- Manual workflow dispatch with environment selection

**Jobs:**

| Job | Description | Runs On |
|-----|-------------|---------|
| `validate` | Lint and validate Bicep templates | Every trigger |
| `what-if-{env}` | Preview infrastructure changes | Based on environment |
| `deploy-{env}` | Deploy infrastructure to Azure | Auto or manual |

**Manual Deployment:**
```bash
# Deploy to dev environment
gh workflow run infrastructure.yml \
  -f environment=dev \
  -f deploy=true

# Deploy to production (requires approval)
gh workflow run infrastructure.yml \
  -f environment=prd \
  -f deploy=true
```

### 4. Application Deployment (`deploy.yml`)

**Triggers:**
- Push to `main` branch (when `src/**` or `clients/web/**` changes)
- Manual workflow dispatch with environment and component selection

**Jobs:**

| Job | Description | Runs On |
|-----|-------------|---------|
| `build-api` | Build and package .NET API | When API deployment is requested |
| `build-web` | Build Angular web application | When web deployment is requested |
| `deploy-{env}` | Deploy to target environment | Based on trigger and environment |

**Manual Deployment:**
```bash
# Deploy everything to stg
gh workflow run deploy.yml \
  -f environment=stg \
  -f deploy_api=true \
  -f deploy_web=true

# Deploy only API to production
gh workflow run deploy.yml \
  -f environment=prd \
  -f deploy_api=true \
  -f deploy_web=false
```

---

## Environment Configuration

### GitHub Environments

Three environments are configured in the GitHub repository:

| Environment | Protection Rules | Deployment |
|-------------|------------------|------------|
| `dev` | None | Automatic on main push |
| `stg` | None | Manual trigger |
| `prd` | Required reviewers | Manual trigger with approval |

### Azure Resources by Environment

| Resource | Dev | Stg | Prd |
|----------|-----|-----|-----|
| **App Service** | `app-lum-dev-api` | `app-lum-stg-api` | `app-lum-prd-api` |
| **Static Web App** | `stapp-lum-dev` | `stapp-lum-stg` | `stapp-lum-prd` |
| **Cosmos DB** | Serverless | Provisioned | Provisioned |
| **Redis** | Basic C0 | Basic C0 | Standard C1 |
| **App Service Plan** | B1 | B1 | P1v3 |

---

## Secrets Management

### Required GitHub Secrets

Configure these secrets in your GitHub repository settings:

#### Azure Authentication (OIDC - Recommended)

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Azure AD application (client) ID |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |

#### Static Web App Tokens

| Secret | Description |
|--------|-------------|
| `AZURE_STATIC_WEB_APP_TOKEN_DEV` | Deployment token for dev SWA |
| `AZURE_STATIC_WEB_APP_TOKEN_STG` | Deployment token for stg SWA |
| `AZURE_STATIC_WEB_APP_TOKEN_PRD` | Deployment token for prd SWA |

### Setting Up Azure OIDC Authentication

1. **Create Azure AD App Registration:**
   ```bash
   az ad app create --display-name "Luminous-GitHub-OIDC"
   ```

2. **Configure Federated Credential:**
   ```bash
   az ad app federated-credential create \
     --id <APP_ID> \
     --parameters '{
       "name": "github-main",
       "issuer": "https://token.actions.githubusercontent.com",
       "subject": "repo:trickpatty/Luminous:ref:refs/heads/main",
       "audiences": ["api://AzureADTokenExchange"]
     }'
   ```

3. **Assign Roles:**
   ```bash
   # Contributor role for deployments
   az role assignment create \
     --assignee <APP_ID> \
     --role "Contributor" \
     --scope /subscriptions/<SUBSCRIPTION_ID>
   ```

---

## Deployment Process

### Automated Deployments

```
main branch push
      |
      v
+------------------+
| .NET Build/Test  |---> Pass ---> Deploy API to Dev
+------------------+
      |
      v
+------------------+
| Angular Build    |---> Pass ---> Deploy Web to Dev
+------------------+
```

### Manual Deployments

1. Navigate to **Actions** tab in GitHub
2. Select the workflow (e.g., "Deploy Application")
3. Click **Run workflow**
4. Select target environment and components
5. Click **Run workflow** button

### Production Deployment Checklist

- [ ] All tests pass on `main` branch
- [ ] Security scans show no critical vulnerabilities
- [ ] Staging deployment verified
- [ ] Change log updated
- [ ] Required reviewers approved

---

## Dependabot Configuration

### Update Schedule

All dependency updates are scheduled for **Monday at 6:00 AM ET**.

### Ecosystem Configuration

| Ecosystem | Directory | Groups |
|-----------|-----------|--------|
| **NuGet** | `/` | Microsoft, Testing, Azure |
| **npm** | `/clients/web` | Angular, Tailwind, Testing |
| **GitHub Actions** | `/` | All actions |
| **Docker** | `/` | All images |

### Handling Updates

1. **Review PR:** Check the changelog and breaking changes
2. **Run Tests:** Ensure all CI checks pass
3. **Merge:** Use squash merge to keep history clean

### Ignoring Updates

To ignore specific updates, add to `.github/dependabot.yml`:

```yaml
ignore:
  - dependency-name: "package-name"
    update-types: ["version-update:semver-major"]
```

---

## Troubleshooting

### Common Issues

#### 1. Azure Login Fails

**Error:** `AADSTS700024: Client assertion is not within its valid time range`

**Solution:** Ensure GitHub Actions runner time is synchronized. Try re-running the job.

#### 2. Bicep Deployment Fails

**Error:** `InvalidTemplateDeployment`

**Solution:**
1. Run `bicep build` locally to validate syntax
2. Check Azure resource quotas
3. Verify parameter file matches template

```bash
# Validate locally
bicep build infra/bicep/main.bicep
bicep build-params infra/bicep/parameters/dev.bicepparam
```

#### 3. Static Web App Deployment Fails

**Error:** `Failed to deploy static web app`

**Solution:**
1. Verify the deployment token is valid
2. Check that the build output matches expected structure
3. Regenerate token if expired

#### 4. .NET Tests Timeout

**Error:** `The operation was canceled`

**Solution:**
1. Check for infinite loops or long-running tests
2. Increase timeout in workflow if needed
3. Split test projects for parallel execution

### Viewing Logs

```bash
# View recent workflow runs
gh run list --workflow=dotnet.yml

# View specific run logs
gh run view <RUN_ID> --log

# Download artifacts
gh run download <RUN_ID>
```

---

## Related Documents

- [Architecture Overview](./ARCHITECTURE.md)
- [Azure Infrastructure](./AZURE-INFRASTRUCTURE.md)
- [Development Setup](./DEVELOPMENT.md)
- [CLAUDE.md (Development Guidelines)](../CLAUDE.md)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-22 | Luminous Team | Initial CI/CD documentation |

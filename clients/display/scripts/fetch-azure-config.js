#!/usr/bin/env node

/**
 * Fetch Azure Configuration Script
 *
 * This script fetches the Azure deployment URLs and updates the environment files
 * for the Luminous display application.
 *
 * Prerequisites:
 *   - Azure CLI installed and authenticated (az login)
 *   - Access to the Luminous Azure resource groups
 *
 * Usage:
 *   node scripts/fetch-azure-config.js <environment>
 *   npm run config:fetch -- <environment>
 *
 * Examples:
 *   npm run config:fetch -- dev
 *   npm run config:fetch -- stg
 *   npm run config:fetch -- prd
 *
 * The script will:
 *   1. Query Azure for the Static Web App and App Service URLs
 *   2. Update the corresponding environment file with the actual URLs
 */

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const ENVIRONMENTS = ['dev', 'stg', 'prd'];
const RG_PREFIX = 'rg-lum';

function runAzCommand(command) {
  try {
    const result = execSync(command, { encoding: 'utf-8', stdio: ['pipe', 'pipe', 'pipe'] });
    return result.trim();
  } catch (error) {
    console.error(`Error running Azure CLI command: ${error.message}`);
    return null;
  }
}

function getResourceGroupName(env) {
  return `${RG_PREFIX}-${env}`;
}

function getStaticWebAppUrl(resourceGroup) {
  const command = `az staticwebapp list --resource-group ${resourceGroup} --query "[0].defaultHostname" -o tsv`;
  return runAzCommand(command);
}

function getAppServiceUrl(resourceGroup) {
  // Filter for App Service (kind=app,linux), not Function Apps
  const command = `az webapp list --resource-group ${resourceGroup} --query "[?contains(kind, 'app') && !contains(kind, 'functionapp')].defaultHostName" -o tsv`;
  const result = runAzCommand(command);
  return result ? result.split('\n')[0] : null;
}

function getEnvironmentFilePath(env) {
  const envFileName = env === 'prd' ? 'environment.production.ts' : `environment.${env}.ts`;
  return path.join(__dirname, '..', 'src', 'environments', envFileName);
}

function updateEnvironmentFile(env, staticWebAppUrl, appServiceUrl) {
  const filePath = getEnvironmentFilePath(env);

  if (!fs.existsSync(filePath)) {
    console.error(`Environment file not found: ${filePath}`);
    return false;
  }

  let content = fs.readFileSync(filePath, 'utf-8');

  // Replace the API URL placeholder
  const oldApiUrlPattern = /apiUrl:\s*'[^']+'/;
  const newApiUrl = `apiUrl: 'https://${staticWebAppUrl}/api'`;
  content = content.replace(oldApiUrlPattern, newApiUrl);

  // Replace the SignalR URL placeholder
  const oldSignalRUrlPattern = /signalRUrl:\s*'[^']+'/;
  const newSignalRUrl = `signalRUrl: 'https://${appServiceUrl}/hubs/sync'`;
  content = content.replace(oldSignalRUrlPattern, newSignalRUrl);

  fs.writeFileSync(filePath, content, 'utf-8');
  return true;
}

function main() {
  const args = process.argv.slice(2);

  if (args.length === 0) {
    console.log('Luminous Display - Azure Configuration Fetcher');
    console.log('');
    console.log('Usage: npm run config:fetch -- <environment>');
    console.log('');
    console.log('Available environments:');
    ENVIRONMENTS.forEach(env => {
      console.log(`  ${env} - ${getResourceGroupName(env)}`);
    });
    console.log('');
    console.log('Examples:');
    console.log('  npm run config:fetch -- dev');
    console.log('  npm run config:fetch -- stg');
    console.log('  npm run config:fetch -- prd');
    console.log('');
    console.log('Prerequisites:');
    console.log('  - Azure CLI installed (az)');
    console.log('  - Authenticated to Azure (az login)');
    console.log('  - Access to the Luminous resource groups');
    process.exit(0);
  }

  const env = args[0].toLowerCase();

  if (!ENVIRONMENTS.includes(env)) {
    console.error(`Invalid environment: ${env}`);
    console.error(`Valid environments: ${ENVIRONMENTS.join(', ')}`);
    process.exit(1);
  }

  console.log(`Fetching Azure configuration for environment: ${env}`);
  console.log('');

  const resourceGroup = getResourceGroupName(env);
  console.log(`Resource group: ${resourceGroup}`);

  // Check if Azure CLI is available
  try {
    execSync('az --version', { stdio: 'pipe' });
  } catch (error) {
    console.error('Azure CLI is not installed or not in PATH.');
    console.error('Please install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli');
    process.exit(1);
  }

  // Check if logged in
  try {
    execSync('az account show', { stdio: 'pipe' });
  } catch (error) {
    console.error('Not logged in to Azure. Please run: az login');
    process.exit(1);
  }

  // Get Static Web App URL
  console.log('Fetching Static Web App URL...');
  const staticWebAppUrl = getStaticWebAppUrl(resourceGroup);
  if (!staticWebAppUrl) {
    console.error(`Could not find Static Web App in resource group: ${resourceGroup}`);
    console.error('Make sure the infrastructure has been deployed.');
    process.exit(1);
  }
  console.log(`  Static Web App: https://${staticWebAppUrl}`);

  // Get App Service URL
  console.log('Fetching App Service URL...');
  const appServiceUrl = getAppServiceUrl(resourceGroup);
  if (!appServiceUrl) {
    console.error(`Could not find App Service in resource group: ${resourceGroup}`);
    console.error('Make sure the infrastructure has been deployed.');
    process.exit(1);
  }
  console.log(`  App Service: https://${appServiceUrl}`);

  console.log('');
  console.log('Updating environment file...');

  if (updateEnvironmentFile(env, staticWebAppUrl, appServiceUrl)) {
    const filePath = getEnvironmentFilePath(env);
    console.log(`  Updated: ${path.relative(process.cwd(), filePath)}`);
    console.log('');
    console.log('Configuration updated successfully!');
    console.log('');
    console.log(`API URL:     https://${staticWebAppUrl}/api`);
    console.log(`SignalR URL: https://${appServiceUrl}/hubs/sync`);
    console.log('');
    console.log(`You can now build the display app for ${env}:`);
    console.log(`  npm run build:${env === 'prd' ? 'prod' : env}`);
  } else {
    console.error('Failed to update environment file.');
    process.exit(1);
  }
}

main();

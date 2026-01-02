/**
 * Production environment configuration
 *
 * IMPORTANT: Update these URLs with your actual Azure deployment URLs.
 * You can find the URLs by running:
 *   az deployment group show --name <deployment-name> --resource-group rg-lum-prd --query properties.outputs
 *
 * Or use the helper script:
 *   npm run config:fetch -- prd
 *
 * URL Pattern (Azure default):
 *   Static Web App: https://stapp-lum-prd-{suffix}.azurestaticapps.net
 *   App Service:    https://app-lum-prd-{suffix}.azurewebsites.net
 *
 * URL Pattern (Custom domain - preferred for production):
 *   Static Web App: https://luminous.app (or your custom domain)
 *   App Service:    https://api.luminous.app (or your custom domain)
 *
 * The {suffix} is a 6-character unique string generated from the resource group ID.
 */
export const environment = {
  production: true,
  environmentName: 'prd',

  // API URL: The Static Web App URL (proxies to App Service backend)
  // Replace with your custom domain or Azure default URL
  apiUrl: 'https://stapp-lum-prd-{SUFFIX}.azurestaticapps.net/api',

  // SignalR URL: Direct connection to App Service for WebSocket support
  // Replace with your custom domain or Azure default URL
  signalRUrl: 'https://app-lum-prd-{SUFFIX}.azurewebsites.net/hubs',

  auth: {
    deviceTokenStorageKey: 'luminous_device_token',
    settingsStorageKey: 'luminous_display_settings',
  },
  display: {
    defaultView: 'schedule',
    refreshInterval: 60000, // 1 minute
    heartbeatInterval: 300000, // 5 minutes
  },
  cache: {
    maxAge: 3600000, // 1 hour
    storeName: 'luminous-display-cache',
  },
};

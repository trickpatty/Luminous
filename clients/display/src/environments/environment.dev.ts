/**
 * Development environment configuration (deployed dev, not local)
 *
 * IMPORTANT: Update these URLs with your actual Azure deployment URLs.
 * You can find the URLs by running:
 *   az deployment group show --name <deployment-name> --resource-group rg-lum-dev --query properties.outputs
 *
 * Or use the helper script:
 *   npm run config:fetch -- dev
 *
 * URL Pattern:
 *   Static Web App: https://stapp-lum-dev-{suffix}.azurestaticapps.net
 *   App Service:    https://app-lum-dev-{suffix}.azurewebsites.net
 *
 * The {suffix} is a 6-character unique string generated from the resource group ID.
 */
export const environment = {
  production: false,
  environmentName: 'dev',

  // API URL: The Static Web App URL (proxies to App Service backend)
  // Replace {SUFFIX} with your actual Azure deployment suffix
  apiUrl: 'https://stapp-lum-dev-{SUFFIX}.azurestaticapps.net/api',

  // SignalR URL: Direct connection to App Service for WebSocket support
  // Replace {SUFFIX} with your actual Azure deployment suffix
  signalRUrl: 'https://app-lum-dev-{SUFFIX}.azurewebsites.net/hubs/sync',

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
  sync: {
    reconnectIntervalMs: 5000,
    maxReconnectAttempts: 10,
    maxReconnectDelayMs: 30000,
  },
};

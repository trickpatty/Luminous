/**
 * Production environment configuration
 */
export const environment = {
  production: true,
  apiUrl: 'https://api.luminous.app/api',
  signalRUrl: 'https://api.luminous.app/hubs',
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

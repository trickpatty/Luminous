/**
 * Local development environment configuration
 *
 * This configuration is used when running locally with `ng serve`.
 * It connects to a local API server running on port 5000.
 *
 * To start the local API:
 *   cd src/Luminous.Api
 *   dotnet run
 */
export const environment = {
  production: false,
  environmentName: 'local',

  // API URL: Local development server
  apiUrl: 'http://localhost:5000/api',

  // SignalR URL: Local development server
  signalRUrl: 'http://localhost:5000/hubs/sync',

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

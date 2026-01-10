/**
 * Production environment configuration
 */
export const environment = {
  production: true,
  apiUrl: '/api',
  signalRUrl: '/hubs/sync',
  webAuthn: {
    rpId: 'luminous.app',
    rpName: 'Luminous Family Hub',
  },
  auth: {
    tokenStorageKey: 'luminous_tokens',
    userStorageKey: 'luminous_user',
  },
  sync: {
    reconnectIntervalMs: 5000,
    maxReconnectAttempts: 10,
    maxReconnectDelayMs: 30000,
  },
};

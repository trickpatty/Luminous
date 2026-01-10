/**
 * Stg environment configuration
 */
export const environment = {
  production: false,
  apiUrl: '/api',
  signalRUrl: '/hubs/sync',
  webAuthn: {
    rpId: 'stg.luminous.app',
    rpName: 'Luminous Family Hub (Stg)',
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

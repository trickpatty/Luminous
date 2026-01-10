/**
 * Dev environment configuration (deployed dev, not local)
 */
export const environment = {
  production: false,
  apiUrl: '/api',
  signalRUrl: '/hubs/sync',
  webAuthn: {
    rpId: 'dev.luminous.app',
    rpName: 'Luminous Family Hub (Dev)',
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

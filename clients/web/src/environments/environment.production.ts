/**
 * Production environment configuration
 */
export const environment = {
  production: true,
  apiUrl: '/api',
  webAuthn: {
    rpId: 'luminous.app',
    rpName: 'Luminous Family Hub',
  },
  auth: {
    tokenStorageKey: 'luminous_tokens',
    userStorageKey: 'luminous_user',
  },
};

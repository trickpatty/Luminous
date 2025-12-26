/**
 * Stg environment configuration
 */
export const environment = {
  production: false,
  apiUrl: '/api',
  webAuthn: {
    rpId: 'stg.luminous.app',
    rpName: 'Luminous Family Hub (Stg)',
  },
  auth: {
    tokenStorageKey: 'luminous_tokens',
    userStorageKey: 'luminous_user',
  },
};

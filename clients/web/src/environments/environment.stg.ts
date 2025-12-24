/**
 * Stg environment configuration
 */
export const environment = {
  production: false,
  apiUrl: 'https://api-stg.luminous.app/api',
  webAuthn: {
    rpId: 'stg.luminous.app',
    rpName: 'Luminous Family Hub (Stg)',
  },
  auth: {
    tokenStorageKey: 'luminous_tokens',
    userStorageKey: 'luminous_user',
  },
};

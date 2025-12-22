/**
 * Staging environment configuration
 */
export const environment = {
  production: false,
  apiUrl: 'https://api-staging.luminous.app/api',
  webAuthn: {
    rpId: 'staging.luminous.app',
    rpName: 'Luminous Family Hub (Staging)',
  },
  auth: {
    tokenStorageKey: 'luminous_tokens',
    userStorageKey: 'luminous_user',
  },
};

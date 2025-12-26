/**
 * Dev environment configuration (deployed dev, not local)
 */
export const environment = {
  production: false,
  apiUrl: '/api',
  webAuthn: {
    rpId: 'dev.luminous.app',
    rpName: 'Luminous Family Hub (Dev)',
  },
  auth: {
    tokenStorageKey: 'luminous_tokens',
    userStorageKey: 'luminous_user',
  },
};

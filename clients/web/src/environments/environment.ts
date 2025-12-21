/**
 * Development environment configuration
 */
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  webAuthn: {
    rpId: 'localhost',
    rpName: 'Luminous Family Hub',
  },
  auth: {
    tokenStorageKey: 'luminous_tokens',
    userStorageKey: 'luminous_user',
  },
};

import { AuthMethod } from './user.model';

/**
 * JWT token pair
 */
export interface TokenPair {
  accessToken: string;
  refreshToken: string;
  expiresIn: number; // seconds
  tokenType: 'Bearer';
}

/**
 * Authentication state
 */
export interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: AuthUserInfo | null;
  tokens: TokenPair | null;
  error: string | null;
}

/**
 * Basic user info from auth token
 */
export interface AuthUserInfo {
  id: string;
  email: string;
  displayName: string;
  familyId: string;
  role: string;
  authMethod: AuthMethod;
  mfaVerified: boolean;
}

/**
 * Login request with email
 */
export interface LoginRequest {
  email: string;
}

/**
 * Email OTP verification request
 */
export interface VerifyOtpRequest {
  email: string;
  code: string;
}

/**
 * Passkey registration options from server
 */
export interface PasskeyRegistrationOptions {
  challenge: string;
  rp: {
    id: string;
    name: string;
  };
  user: {
    id: string;
    name: string;
    displayName: string;
  };
  pubKeyCredParams: PublicKeyCredentialParameters[];
  timeout?: number;
  attestation?: AttestationConveyancePreference;
  authenticatorSelection?: AuthenticatorSelectionCriteria;
}

/**
 * Passkey authentication options from server
 */
export interface PasskeyAuthenticationOptions {
  challenge: string;
  rpId: string;
  timeout?: number;
  userVerification?: UserVerificationRequirement;
  allowCredentials?: PublicKeyCredentialDescriptor[];
}

/**
 * Passkey registration response to send to server
 */
export interface PasskeyRegistrationResponse {
  id: string;
  rawId: string;
  type: 'public-key';
  response: {
    clientDataJSON: string;
    attestationObject: string;
    transports?: AuthenticatorTransport[];
  };
}

/**
 * Passkey authentication response to send to server
 */
export interface PasskeyAuthenticationResponse {
  id: string;
  rawId: string;
  type: 'public-key';
  response: {
    clientDataJSON: string;
    authenticatorData: string;
    signature: string;
    userHandle?: string;
  };
}

/**
 * User registration request (deprecated - use RegisterStartRequest)
 * @deprecated Use RegisterStartRequest and RegisterCompleteRequest for secure 2-step registration
 */
export interface RegisterRequest {
  email: string;
  displayName: string;
  familyName?: string;
  inviteCode?: string;
  timezone?: string;
}

/**
 * Request to start registration (step 1 of 2)
 */
export interface RegisterStartRequest {
  email: string;
  displayName: string;
  familyName: string;
  timezone?: string;
}

/**
 * Result from starting registration
 */
export interface RegisterStartResult {
  success: boolean;
  sessionId?: string;
  message: string;
  maskedEmail: string;
  expiresAt?: string;
  retryAfterSeconds?: number;
}

/**
 * Request to complete registration (step 2 of 2)
 */
export interface RegisterCompleteRequest {
  sessionId: string;
  email: string;
  code: string;
}

/**
 * Result from completing registration
 */
export interface RegisterCompleteResult {
  success: boolean;
  error?: string;
  family?: {
    id: string;
    name: string;
    timezone: string;
    memberCount: number;
    deviceCount: number;
  };
  auth?: TokenPair;
  remainingAttempts: number;
}

/**
 * Family creation result from registration (deprecated)
 * @deprecated Use RegisterCompleteResult instead
 */
export interface FamilyCreationResult {
  family: {
    id: string;
    name: string;
    timezone: string;
    memberCount: number;
    deviceCount: number;
  };
  auth: TokenPair;
}

/**
 * Family creation request
 */
export interface CreateFamilyRequest {
  name: string;
  timezone: string;
}

/**
 * Passkey credential info
 */
export interface PasskeyCredential {
  id: string;
  name: string;
  createdAt: string;
  lastUsedAt?: string;
  transports?: AuthenticatorTransport[];
}

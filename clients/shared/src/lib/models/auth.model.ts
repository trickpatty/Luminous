import { AuthMethod, UserProfile } from './user.model';

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
  profile?: UserProfile;
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
 * Request to start registration (step 1 of 2)
 */
export interface RegisterStartRequest {
  email: string;
  displayName: string;
  familyName: string;
  timezone?: string;
  inviteCode?: string;
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
 * Result from OTP verification
 */
export interface OtpVerifyResult {
  success: boolean;
  auth?: TokenPair;
  error?: string;
  remainingAttempts: number;
}

/**
 * Request to complete registration (step 2 of 2)
 * Note: Email is not included as it's retrieved from the secure server-side session
 */
export interface RegisterCompleteRequest {
  sessionId: string;
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
 * Family creation request
 */
export interface CreateFamilyRequest {
  name: string;
  timezone: string;
}

/**
 * Passkey credential info (frontend format)
 */
export interface PasskeyCredential {
  id: string;
  name: string;
  createdAt: string;
  lastUsedAt?: string;
  transports?: AuthenticatorTransport[];
}

/**
 * Passkey DTO from API (backend format)
 */
export interface PasskeyDto {
  id: string;
  credentialId: string;
  displayName?: string;
  registeredAt: string;
  lastUsedAt?: string;
  aaGuid: string;
  transports: string[];
  isActive: boolean;
}

/**
 * API response for passkey list endpoint
 */
export interface PasskeyListResponse {
  passkeys: PasskeyDto[];
  totalCount: number;
}

/**
 * API response from passkey registration start endpoint.
 * Note: The options field is a JSON string that needs to be parsed.
 */
export interface PasskeyRegisterStartResult {
  options: string; // JSON string of PasskeyRegistrationOptions
  sessionId: string;
}

/**
 * Parsed result from passkey registration start, ready for WebAuthn API
 */
export interface PasskeyRegistrationStartData {
  options: PasskeyRegistrationOptions;
  sessionId: string;
}

/**
 * API response from passkey authentication start endpoint.
 * Note: The options field is a JSON string that needs to be parsed.
 */
export interface PasskeyAuthenticateStartResult {
  options: string; // JSON string of PasskeyAuthenticationOptions
  sessionId: string;
}

/**
 * Parsed result from passkey authentication start, ready for WebAuthn API
 */
export interface PasskeyAuthenticationStartData {
  options: PasskeyAuthenticationOptions;
  sessionId: string;
}

/**
 * Complete passkey registration request to server
 */
export interface PasskeyRegisterCompleteRequest {
  sessionId: string;
  attestationResponse: PasskeyRegistrationResponse;
  displayName?: string;
}

/**
 * Complete passkey authentication request to server
 */
export interface PasskeyAuthenticateCompleteRequest {
  sessionId: string;
  assertionResponse: PasskeyAuthenticationResponse;
}

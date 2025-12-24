/**
 * User role within a family
 */
export enum UserRole {
  Owner = 'Owner',
  Admin = 'Admin',
  Adult = 'Adult',
  Teen = 'Teen',
  Child = 'Child',
  Caregiver = 'Caregiver',
}

/**
 * User profile preferences
 */
export interface UserProfile {
  color: string;
  avatarUrl?: string;
  birthDate?: string;
  nickname?: string;
  showAge?: boolean;
  age?: number;
}

/**
 * Caregiver-specific information
 */
export interface CaregiverInfo {
  allergies?: string[];
  medicalNotes?: string;
  emergencyContacts?: EmergencyContact[];
  importantNotes?: string;
}

/**
 * Emergency contact information
 */
export interface EmergencyContact {
  name: string;
  relationship: string;
  phone: string;
  isPrimary: boolean;
}

/**
 * User entity
 */
export interface User {
  id: string;
  familyId: string;
  email: string;
  displayName: string;
  role: UserRole;
  profile: UserProfile;
  caregiverInfo?: CaregiverInfo;
  emailVerified?: boolean;
  lastLoginAt?: string;
  isActive?: boolean;
  createdAt: string;
  updatedAt?: string;
}

/**
 * Request to update user profile
 */
export interface UpdateUserProfileRequest {
  displayName?: string;
  profile?: Partial<UserProfile>;
}

/**
 * Request to update user role
 */
export interface UpdateUserRoleRequest {
  newRole: UserRole;
}

/**
 * Request to update caregiver info
 */
export interface UpdateCaregiverInfoRequest {
  allergies?: string[];
  medicalNotes?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  doctorName?: string;
  doctorPhone?: string;
  schoolName?: string;
  notes?: string;
}

/**
 * Caregiver access token
 */
export interface CaregiverAccessToken {
  token: string;
  accessUrl: string;
  expiresAt: string;
}

/**
 * Current authenticated user with auth metadata
 */
export interface AuthenticatedUser extends User {
  authMethod: AuthMethod;
  mfaVerified: boolean;
  lastLoginAt: string;
}

/**
 * Authentication method used for login
 */
export enum AuthMethod {
  Passkey = 'passkey',
  EmailOtp = 'email_otp',
  Password = 'password',
  Social = 'social',
}

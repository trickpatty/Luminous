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
  caregiverInfo?: CaregiverInfo;
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
  createdAt: string;
  updatedAt?: string;
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

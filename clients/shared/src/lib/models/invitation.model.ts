import { UserRole } from './user.model';

/**
 * Invitation status enumeration
 */
export enum InvitationStatus {
  Pending = 'Pending',
  Accepted = 'Accepted',
  Declined = 'Declined',
  Expired = 'Expired',
  Revoked = 'Revoked',
}

/**
 * Invitation entity
 */
export interface Invitation {
  id: string;
  familyId: string;
  familyName: string;
  email: string;
  role: UserRole;
  code: string;
  expiresAt: string;
  status: InvitationStatus;
  message?: string;
  createdAt: string;
  createdBy?: string;
  isValid: boolean;
  acceptedAt?: string;
  acceptedUserId?: string;
}

/**
 * Request to send an invitation
 */
export interface SendInvitationRequest {
  email: string;
  role: UserRole;
  message?: string;
}

/**
 * Request to accept an invitation
 */
export interface AcceptInvitationRequest {
  displayName: string;
  nickname?: string;
  avatarUrl?: string;
  color?: string;
}

/**
 * Result of accepting an invitation
 */
export interface AcceptedInvitationResult {
  user: {
    id: string;
    familyId: string;
    email: string;
    displayName: string;
    role: string;
  };
  accessToken: string;
  refreshToken?: string;
  tokenType: string;
  expiresIn: number;
}

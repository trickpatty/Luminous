import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import {
  AuthService,
  UserService,
  InvitationService,
} from '../../../../core';
import {
  CardComponent,
  ButtonComponent,
  AlertComponent,
  AvatarComponent,
  SpinnerComponent,
} from '../../../../shared';
import { User, UserRole, Invitation, InvitationStatus } from '../../../../models';

@Component({
  selector: 'app-members',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CardComponent,
    ButtonComponent,
    AlertComponent,
    AvatarComponent,
    SpinnerComponent,
  ],
  template: `
    <div class="p-4 sm:p-6 lg:p-8 max-w-6xl mx-auto">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h2 class="text-2xl font-semibold text-gray-900">Family Members</h2>
          <p class="mt-1 text-gray-600">
            Manage your family members and send invitations.
          </p>
        </div>
        @if (canManageMembers()) {
          <app-button variant="primary" (onClick)="showInviteModal.set(true)">
            <svg class="h-5 w-5 mr-2 -ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
            </svg>
            Invite Member
          </app-button>
        }
      </div>

      @if (error()) {
        <app-alert variant="error" [dismissible]="true" (dismiss)="error.set(null)" class="mb-6">
          {{ error() }}
        </app-alert>
      }

      @if (successMessage()) {
        <app-alert variant="success" [dismissible]="true" (dismiss)="successMessage.set(null)" class="mb-6">
          {{ successMessage() }}
        </app-alert>
      }

      <!-- Members List -->
      <app-card title="Members" [subtitle]="members().length + ' member(s)'">
        @if (userService.loading()) {
          <div class="flex justify-center py-8">
            <app-spinner size="lg" label="Loading members..." />
          </div>
        } @else if (members().length === 0) {
          <div class="text-center py-8 text-gray-500">
            <p>No members found.</p>
          </div>
        } @else {
          <div class="divide-y divide-gray-100">
            @for (member of members(); track member.id) {
              <div class="flex items-center justify-between py-4 px-2 hover:bg-gray-50 rounded-lg -mx-2">
                <a [routerLink]="['/dashboard/profile', member.id]" class="flex items-center gap-4 flex-1 min-w-0">
                  <app-avatar
                    [name]="member.displayName"
                    [color]="member.profile.color"
                    size="md"
                  />
                  <div class="min-w-0">
                    <p class="text-sm font-medium text-gray-900 hover:text-primary-600 transition-colors">
                      {{ member.displayName }}
                      @if (member.id === currentUserId()) {
                        <span class="text-xs text-gray-500 ml-1">(You)</span>
                      }
                    </p>
                    <p class="text-sm text-gray-500 truncate">{{ member.email }}</p>
                  </div>
                </a>
                <div class="flex items-center gap-3">
                  <span
                    class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                    [class]="getRoleBadgeClass(member.role)"
                  >
                    {{ member.role }}
                  </span>
                  @if (canManageMembers() && member.id !== currentUserId() && member.role !== 'Owner') {
                    <div class="relative">
                      <button
                        type="button"
                        class="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg"
                        (click)="toggleMemberMenu(member.id)"
                      >
                        <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z" />
                        </svg>
                      </button>
                      @if (openMemberMenu() === member.id) {
                        <div class="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 z-10">
                          <div class="py-1">
                            <button
                              type="button"
                              class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                              (click)="editMemberRole(member)"
                            >
                              Change Role
                            </button>
                            <button
                              type="button"
                              class="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50"
                              (click)="confirmRemoveMember(member)"
                            >
                              Remove from Family
                            </button>
                          </div>
                        </div>
                      }
                    </div>
                  }
                </div>
              </div>
            }
          </div>
        }
      </app-card>

      <!-- Pending Invitations -->
      @if (canManageMembers()) {
        <app-card
          title="Pending Invitations"
          [subtitle]="pendingInvitations().length + ' pending'"
          class="mt-6"
        >
          @if (invitationService.loading()) {
            <div class="flex justify-center py-8">
              <app-spinner size="md" />
            </div>
          } @else if (pendingInvitations().length === 0) {
            <div class="text-center py-6 text-gray-500">
              <p>No pending invitations.</p>
            </div>
          } @else {
            <div class="divide-y divide-gray-100">
              @for (invitation of pendingInvitations(); track invitation.id) {
                <div class="flex items-center justify-between py-4">
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-gray-900">{{ invitation.email }}</p>
                    <div class="flex items-center gap-2 mt-1">
                      <code class="text-xs font-mono bg-gray-100 px-2 py-0.5 rounded">
                        {{ invitation.code }}
                      </code>
                      <button
                        type="button"
                        (click)="copyCode(invitation.code)"
                        class="text-xs text-primary-600 hover:text-primary-700"
                      >
                        Copy
                      </button>
                    </div>
                    <p class="text-xs text-gray-500 mt-1">
                      Invited as {{ invitation.role }} &middot;
                      Expires {{ formatDate(invitation.expiresAt) }}
                    </p>
                  </div>
                  <app-button
                    variant="ghost"
                    size="sm"
                    (onClick)="revokeInvitation(invitation)"
                  >
                    Revoke
                  </app-button>
                </div>
              }
            </div>
          }
        </app-card>
      }

      <!-- Invite Modal -->
      @if (showInviteModal()) {
        <div class="fixed inset-0 z-50 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="fixed inset-0 bg-gray-500 bg-opacity-75" (click)="closeInviteModal()"></div>
            <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Invite Family Member</h3>

              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">
                    Email Address
                  </label>
                  <input
                    type="email"
                    [(ngModel)]="inviteEmail"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    placeholder="email@example.com"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">
                    Role
                  </label>
                  <select
                    [(ngModel)]="inviteRole"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  >
                    @for (role of assignableRoles(); track role) {
                      <option [value]="role">{{ role }}</option>
                    }
                  </select>
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">
                    Personal Message (optional)
                  </label>
                  <textarea
                    [(ngModel)]="inviteMessage"
                    rows="2"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    placeholder="Add a personal message..."
                  ></textarea>
                </div>
              </div>

              <div class="flex justify-end gap-3 mt-6">
                <app-button variant="secondary" (onClick)="closeInviteModal()">
                  Cancel
                </app-button>
                <app-button
                  variant="primary"
                  [loading]="sendingInvite()"
                  [disabled]="!inviteEmail"
                  (onClick)="sendInvitation()"
                >
                  Send Invitation
                </app-button>
              </div>
            </div>
          </div>
        </div>
      }

      <!-- Change Role Modal -->
      @if (showRoleModal()) {
        <div class="fixed inset-0 z-50 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="fixed inset-0 bg-gray-500 bg-opacity-75" (click)="closeRoleModal()"></div>
            <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Change Role</h3>
              <p class="text-sm text-gray-600 mb-4">
                Change role for {{ selectedMember()?.displayName }}
              </p>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">
                  New Role
                </label>
                <select
                  [(ngModel)]="newRole"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                >
                  @for (role of assignableRoles(); track role) {
                    <option [value]="role">{{ role }}</option>
                  }
                </select>
              </div>

              <div class="flex justify-end gap-3 mt-6">
                <app-button variant="secondary" (onClick)="closeRoleModal()">
                  Cancel
                </app-button>
                <app-button
                  variant="primary"
                  [loading]="updatingRole()"
                  (onClick)="updateRole()"
                >
                  Update Role
                </app-button>
              </div>
            </div>
          </div>
        </div>
      }

      <!-- Remove Member Confirmation Modal -->
      @if (showRemoveModal()) {
        <div class="fixed inset-0 z-50 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="fixed inset-0 bg-gray-500 bg-opacity-75" (click)="closeRemoveModal()"></div>
            <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Remove Family Member</h3>
              <p class="text-sm text-gray-600 mb-4">
                Are you sure you want to remove <strong>{{ selectedMember()?.displayName }}</strong> from your family?
                This action cannot be undone.
              </p>

              <div class="flex justify-end gap-3 mt-6">
                <app-button variant="secondary" (onClick)="closeRemoveModal()">
                  Cancel
                </app-button>
                <app-button
                  variant="danger"
                  [loading]="removingMember()"
                  (onClick)="removeMember()"
                >
                  Remove Member
                </app-button>
              </div>
            </div>
          </div>
        </div>
      }

      <!-- Access Code Modal -->
      @if (showCodeModal() && lastCreatedInvitation()) {
        <div class="fixed inset-0 z-50 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="fixed inset-0 bg-gray-500 bg-opacity-75" (click)="closeCodeModal()"></div>
            <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6">
              <div class="text-center mb-6">
                <div class="mx-auto w-12 h-12 bg-green-100 rounded-full flex items-center justify-center mb-4">
                  <svg class="w-6 h-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                  </svg>
                </div>
                <h3 class="text-lg font-semibold text-gray-900">Invitation Sent!</h3>
                <p class="text-sm text-gray-600 mt-1">
                  An invitation has been sent to <strong>{{ lastCreatedInvitation()!.email }}</strong>
                </p>
              </div>

              <div class="space-y-4">
                <!-- Access Code -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">
                    Access Code
                  </label>
                  <div class="flex items-center gap-2">
                    <code class="flex-1 px-4 py-3 bg-gray-100 rounded-lg font-mono text-lg text-center tracking-wider">
                      {{ lastCreatedInvitation()!.code }}
                    </code>
                    <button
                      type="button"
                      (click)="copyCode(lastCreatedInvitation()!.code)"
                      class="p-3 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
                      [title]="codeCopied() ? 'Copied!' : 'Copy code'"
                    >
                      @if (codeCopied()) {
                        <svg class="w-5 h-5 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                        </svg>
                      } @else {
                        <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                        </svg>
                      }
                    </button>
                  </div>
                  <p class="text-xs text-gray-500 mt-1">
                    Share this code with {{ lastCreatedInvitation()!.email }} to let them join your family.
                  </p>
                </div>

                <!-- Invite Link -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">
                    Or share invite link
                  </label>
                  <div class="flex items-center gap-2">
                    <input
                      type="text"
                      readonly
                      [value]="getInviteUrl(lastCreatedInvitation()!.code)"
                      class="flex-1 px-3 py-2 bg-gray-50 border border-gray-200 rounded-lg text-sm text-gray-600"
                    />
                    <button
                      type="button"
                      (click)="copyInviteUrl(lastCreatedInvitation()!.code)"
                      class="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
                      title="Copy link"
                    >
                      <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
                      </svg>
                    </button>
                  </div>
                </div>

                <!-- Expiration notice -->
                <div class="bg-yellow-50 border border-yellow-200 rounded-lg p-3">
                  <div class="flex items-start gap-2">
                    <svg class="w-5 h-5 text-yellow-600 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                    </svg>
                    <div class="text-sm text-yellow-800">
                      <p class="font-medium">This invitation expires on {{ formatDate(lastCreatedInvitation()!.expiresAt) }}</p>
                      <p class="text-yellow-700 mt-0.5">The invitee will need to create their account before then.</p>
                    </div>
                  </div>
                </div>
              </div>

              <div class="flex justify-end mt-6">
                <app-button variant="primary" (onClick)="closeCodeModal()">
                  Done
                </app-button>
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `,
})
export class MembersComponent implements OnInit {
  private readonly authService = inject(AuthService);
  readonly userService = inject(UserService);
  readonly invitationService = inject(InvitationService);

  // State
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  showInviteModal = signal(false);
  showRoleModal = signal(false);
  showRemoveModal = signal(false);
  showCodeModal = signal(false);
  sendingInvite = signal(false);
  updatingRole = signal(false);
  removingMember = signal(false);
  openMemberMenu = signal<string | null>(null);
  selectedMember = signal<User | null>(null);
  lastCreatedInvitation = signal<Invitation | null>(null);
  codeCopied = signal(false);

  // Form fields
  inviteEmail = '';
  inviteRole: UserRole = UserRole.Adult;
  inviteMessage = '';
  newRole: UserRole = UserRole.Adult;

  // Computed
  members = this.userService.members;
  currentUserId = () => this.authService.user()?.id;

  pendingInvitations = () =>
    this.invitationService
      .invitations()
      .filter((i) => i.status === InvitationStatus.Pending);

  canManageMembers = () => {
    const role = this.authService.user()?.role;
    return role === 'Owner' || role === 'Admin';
  };

  assignableRoles = () => {
    const currentRole = this.authService.user()?.role as UserRole;
    return this.userService.getAssignableRoles(currentRole);
  };

  ngOnInit(): void {
    this.loadData();

    // Close menu when clicking outside
    document.addEventListener('click', () => {
      this.openMemberMenu.set(null);
    });
  }

  private loadData(): void {
    const familyId = this.authService.user()?.familyId;
    if (familyId) {
      this.userService.getMembers(familyId).subscribe();
      if (this.canManageMembers()) {
        this.invitationService.getPendingInvitations(familyId).subscribe();
      }
    }
  }

  toggleMemberMenu(memberId: string): void {
    event?.stopPropagation();
    this.openMemberMenu.update((current) =>
      current === memberId ? null : memberId
    );
  }

  getRoleBadgeClass(role: UserRole): string {
    const classes: Record<string, string> = {
      Owner: 'bg-purple-100 text-purple-700',
      Admin: 'bg-blue-100 text-blue-700',
      Adult: 'bg-green-100 text-green-700',
      Teen: 'bg-yellow-100 text-yellow-700',
      Child: 'bg-orange-100 text-orange-700',
      Caregiver: 'bg-gray-100 text-gray-700',
    };
    return classes[role] || 'bg-gray-100 text-gray-700';
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString();
  }

  // Invite Modal
  closeInviteModal(): void {
    this.showInviteModal.set(false);
    this.inviteEmail = '';
    this.inviteRole = UserRole.Adult;
    this.inviteMessage = '';
  }

  sendInvitation(): void {
    const familyId = this.authService.user()?.familyId;
    if (!familyId || !this.inviteEmail) return;

    this.sendingInvite.set(true);
    this.error.set(null);

    this.invitationService
      .sendInvitation(familyId, {
        email: this.inviteEmail,
        role: this.inviteRole,
        message: this.inviteMessage || undefined,
      })
      .subscribe({
        next: (invitation) => {
          this.sendingInvite.set(false);
          this.closeInviteModal();
          this.lastCreatedInvitation.set(invitation);
          this.showCodeModal.set(true);
        },
        error: (err) => {
          this.sendingInvite.set(false);
          this.error.set(err.message || 'Failed to send invitation');
        },
      });
  }

  getInviteUrl(code: string): string {
    const baseUrl = window.location.origin;
    return `${baseUrl}/auth/join/${code}`;
  }

  async copyCode(code: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(code);
      this.codeCopied.set(true);
      setTimeout(() => this.codeCopied.set(false), 2000);
    } catch {
      this.error.set('Failed to copy code');
    }
  }

  async copyInviteUrl(code: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.getInviteUrl(code));
      this.codeCopied.set(true);
      setTimeout(() => this.codeCopied.set(false), 2000);
    } catch {
      this.error.set('Failed to copy link');
    }
  }

  closeCodeModal(): void {
    this.showCodeModal.set(false);
    this.lastCreatedInvitation.set(null);
    this.codeCopied.set(false);
    this.successMessage.set('Invitation sent successfully!');
  }

  revokeInvitation(invitation: Invitation): void {
    const familyId = this.authService.user()?.familyId;
    if (!familyId) return;

    this.invitationService.revokeInvitation(familyId, invitation.id).subscribe({
      next: () => {
        this.successMessage.set('Invitation revoked.');
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to revoke invitation');
      },
    });
  }

  // Role Modal
  editMemberRole(member: User): void {
    this.selectedMember.set(member);
    this.newRole = member.role;
    this.openMemberMenu.set(null);
    this.showRoleModal.set(true);
  }

  closeRoleModal(): void {
    this.showRoleModal.set(false);
    this.selectedMember.set(null);
  }

  updateRole(): void {
    const familyId = this.authService.user()?.familyId;
    const member = this.selectedMember();
    if (!familyId || !member) return;

    this.updatingRole.set(true);
    this.error.set(null);

    this.userService
      .updateRole(familyId, member.id, { newRole: this.newRole })
      .subscribe({
        next: () => {
          this.updatingRole.set(false);
          this.closeRoleModal();
          this.successMessage.set('Role updated successfully!');
        },
        error: (err) => {
          this.updatingRole.set(false);
          this.error.set(err.message || 'Failed to update role');
        },
      });
  }

  // Remove Modal
  confirmRemoveMember(member: User): void {
    this.selectedMember.set(member);
    this.openMemberMenu.set(null);
    this.showRemoveModal.set(true);
  }

  closeRemoveModal(): void {
    this.showRemoveModal.set(false);
    this.selectedMember.set(null);
  }

  removeMember(): void {
    const familyId = this.authService.user()?.familyId;
    const member = this.selectedMember();
    if (!familyId || !member) return;

    this.removingMember.set(true);
    this.error.set(null);

    this.userService.removeUser(familyId, member.id).subscribe({
      next: () => {
        this.removingMember.set(false);
        this.closeRemoveModal();
        this.successMessage.set('Member removed from family.');
      },
      error: (err) => {
        this.removingMember.set(false);
        this.error.set(err.message || 'Failed to remove member');
      },
    });
  }
}

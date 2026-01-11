import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService, CalendarConnectionService, UserService, ToastService } from '../../../../core';
import {
  CardComponent,
  ButtonComponent,
  AlertComponent,
  SpinnerComponent,
  AvatarComponent,
  AvatarGroupComponent,
} from '../../../../shared';
import {
  CalendarConnection,
  CalendarProvider,
  CalendarConnectionStatus,
  ExternalCalendarInfo,
  User,
} from '../../../../models';

type ModalStep = 'provider' | 'oauth-loading' | 'popup-blocked' | 'select-calendars' | 'assign-members' | 'ics-url' | 'success';

@Component({
  selector: 'app-calendars',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardComponent,
    ButtonComponent,
    AlertComponent,
    SpinnerComponent,
    AvatarComponent,
    AvatarGroupComponent,
  ],
  template: `
    <div class="p-4 sm:p-6 lg:p-8 max-w-6xl mx-auto">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h2 class="text-2xl font-semibold text-gray-900">Connected Calendars</h2>
          <p class="mt-1 text-gray-600">
            Sync calendars from Google, Outlook, or any calendar app.
          </p>
        </div>
        @if (canManageCalendars()) {
          <app-button variant="primary" (onClick)="openConnectModal()">
            <svg class="h-5 w-5 mr-2 -ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            Connect Calendar
          </app-button>
        }
      </div>

      <!-- Alerts -->
      @if (error()) {
        <app-alert variant="error" [dismissible]="true" (dismiss)="clearError()" class="mb-6">
          {{ error() }}
        </app-alert>
      }
      @if (successMessage()) {
        <app-alert variant="success" [dismissible]="true" (dismiss)="successMessage.set(null)" class="mb-6">
          {{ successMessage() }}
        </app-alert>
      }

      <!-- Loading State -->
      @if (calendarService.loading() && connections().length === 0) {
        <div class="flex justify-center py-12">
          <app-spinner size="lg" label="Loading calendars..." />
        </div>
      }
      <!-- Empty State -->
      @else if (connections().length === 0) {
        <app-card>
          <div class="text-center py-12">
            <svg class="mx-auto h-16 w-16 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <h3 class="mt-4 text-lg font-medium text-gray-900">Connect your calendars</h3>
            <p class="mt-2 text-gray-500 max-w-sm mx-auto">
              Sync events from Google, Outlook, or any calendar app to see everyone's schedule in one place.
            </p>
            @if (canManageCalendars()) {
              <app-button variant="primary" class="mt-6" (onClick)="openConnectModal()">
                Connect Your First Calendar
              </app-button>
            }
          </div>
        </app-card>
      }
      <!-- Calendar Grid -->
      @else {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          @for (connection of connections(); track connection.id) {
            <div class="bg-white rounded-xl border border-gray-200 hover:shadow-md transition-shadow overflow-visible">
              <div class="p-5 overflow-visible">
                <!-- Header -->
                <div class="flex items-start justify-between gap-2 mb-4">
                  <div class="flex items-center gap-3 min-w-0 overflow-hidden">
                    <div
                      class="w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0"
                      [class]="getProviderBgColor(connection.provider)"
                    >
                      <span [innerHTML]="getProviderIcon(connection.provider)" class="w-5 h-5"></span>
                    </div>
                    <div class="min-w-0">
                      <h3 class="text-sm font-semibold text-gray-900 truncate">{{ connection.name }}</h3>
                      <p class="text-xs text-gray-500 truncate">
                        {{ connection.externalAccountId || calendarService.getProviderDisplayName(connection.provider) }}
                      </p>
                    </div>
                  </div>
                  <!-- Actions Menu -->
                  @if (canManageCalendars()) {
                    <div class="relative flex-shrink-0">
                      <button
                        type="button"
                        data-menu-button
                        class="flex items-center justify-center w-8 h-8 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
                        (click)="toggleMenu(connection.id); $event.stopPropagation()"
                      >
                        <svg class="w-5 h-5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                          <path d="M10 6a2 2 0 110-4 2 2 0 010 4zM10 12a2 2 0 110-4 2 2 0 010 4zM10 18a2 2 0 110-4 2 2 0 010 4z" />
                        </svg>
                      </button>
                      @if (openMenuId() === connection.id) {
                        <div data-menu-dropdown class="absolute right-0 top-full mt-1 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-50">
                          <button
                            type="button"
                            class="w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
                            (click)="syncCalendar(connection)"
                          >
                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                            </svg>
                            Sync Now
                          </button>
                          <button
                            type="button"
                            class="w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
                            (click)="openEditModal(connection)"
                          >
                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                            Settings
                          </button>
                          <div class="border-t border-gray-100 my-1"></div>
                          <button
                            type="button"
                            class="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
                            (click)="confirmDisconnect(connection)"
                          >
                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7a4 4 0 11-8 0 4 4 0 018 0zM9 14a6 6 0 00-6 6v1h12v-1a6 6 0 00-6-6zM21 12h-6" />
                            </svg>
                            Disconnect
                          </button>
                        </div>
                      }
                    </div>
                  }
                </div>

                <!-- Assigned Members -->
                <div class="mb-4">
                  @if (getAssignedMembers(connection).length > 0) {
                    <div class="flex items-center gap-2">
                      <app-avatar-group [max]="3">
                        @for (member of getAssignedMembers(connection); track member.id) {
                          <app-avatar
                            [name]="member.displayName"
                            [color]="member.profile.color"
                            [src]="member.profile.avatarUrl"
                            size="xs"
                          />
                        }
                      </app-avatar-group>
                      <span class="text-xs text-gray-500">
                        {{ getAssignedMemberNames(connection) }}
                      </span>
                    </div>
                  } @else {
                    <span class="text-xs text-gray-400 italic">No members assigned</span>
                  }
                </div>

                <!-- Status & Sync Info -->
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-2">
                    @if (calendarService.isConnectionSyncing(connection.id)) {
                      <span class="inline-flex items-center gap-1.5 text-xs text-blue-600">
                        <svg class="w-3.5 h-3.5 animate-spin" fill="none" viewBox="0 0 24 24">
                          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        Syncing...
                      </span>
                    } @else {
                      <span
                        class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium"
                        [class]="calendarService.getStatusInfo(connection.status).bgColor + ' ' + calendarService.getStatusInfo(connection.status).color"
                      >
                        @if (connection.status === CalendarConnectionStatus.Active) {
                          <span class="w-1.5 h-1.5 rounded-full bg-green-500"></span>
                        }
                        {{ calendarService.getStatusInfo(connection.status).label }}
                      </span>
                    }
                  </div>
                  @if (connection.lastSyncedAt) {
                    <span class="text-xs text-gray-500">
                      {{ formatLastSync(connection.lastSyncedAt) }}
                    </span>
                  }
                </div>

                <!-- Error Message -->
                @if (connection.status === CalendarConnectionStatus.AuthError) {
                  <div class="mt-3 p-2 bg-orange-50 rounded-lg">
                    <p class="text-xs text-orange-700">
                      Your connection needs to be re-authorized.
                    </p>
                    <button
                      type="button"
                      class="mt-1 text-xs font-medium text-orange-700 hover:text-orange-800 underline"
                      (click)="reconnectCalendar(connection)"
                    >
                      Reconnect
                    </button>
                  </div>
                }
                @if (connection.status === CalendarConnectionStatus.SyncError && connection.lastSyncError) {
                  <div class="mt-3 p-2 bg-red-50 rounded-lg">
                    <p class="text-xs text-red-700">{{ connection.lastSyncError }}</p>
                  </div>
                }
              </div>
            </div>
          }
        </div>
      }

      <!-- Connect Calendar Modal -->
      @if (showConnectModal()) {
        <div class="fixed inset-0 z-50 overflow-y-auto" (click)="closeConnectModal()">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="modal-overlay"></div>
            <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full" (click)="$event.stopPropagation()">
              <!-- Provider Selection -->
              @if (modalStep() === 'provider') {
                <div class="p-6">
                  <div class="flex items-center justify-between mb-6">
                    <h3 class="text-lg font-semibold text-gray-900">Connect a Calendar</h3>
                    <button
                      type="button"
                      class="p-1 text-gray-400 hover:text-gray-600 rounded-lg"
                      (click)="closeConnectModal()"
                    >
                      <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>

                  <div class="space-y-3">
                    <!-- Google Calendar -->
                    <button
                      type="button"
                      class="w-full flex items-center gap-4 p-4 border border-gray-200 rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-colors text-left"
                      (click)="startOAuthFlow(CalendarProvider.Google)"
                    >
                      <div class="w-10 h-10 bg-red-100 rounded-lg flex items-center justify-center flex-shrink-0">
                        <svg class="w-5 h-5 text-red-600" viewBox="0 0 24 24" fill="currentColor">
                          <path d="M12.545,10.239v3.821h5.445c-0.712,2.315-2.647,3.972-5.445,3.972c-3.332,0-6.033-2.701-6.033-6.032s2.701-6.032,6.033-6.032c1.498,0,2.866,0.549,3.921,1.453l2.814-2.814C17.503,2.988,15.139,2,12.545,2C7.021,2,2.543,6.477,2.543,12s4.478,10,10.002,10c8.396,0,10.249-7.85,9.426-11.748L12.545,10.239z"/>
                        </svg>
                      </div>
                      <div class="flex-1 min-w-0">
                        <p class="font-medium text-gray-900">Google Calendar</p>
                        <p class="text-sm text-gray-500">Personal, work, and shared calendars</p>
                      </div>
                      <svg class="w-5 h-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                      </svg>
                    </button>

                    <!-- Microsoft Outlook -->
                    <button
                      type="button"
                      class="w-full flex items-center gap-4 p-4 border border-gray-200 rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-colors text-left"
                      (click)="startOAuthFlow(CalendarProvider.Outlook)"
                    >
                      <div class="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center flex-shrink-0">
                        <svg class="w-5 h-5 text-blue-600" viewBox="0 0 24 24" fill="currentColor">
                          <path d="M11.5,3v8.5H3V3H11.5z M11.5,21H3v-8.5h8.5V21z M12.5,3H21v8.5h-8.5V3z M21,12.5V21h-8.5v-8.5H21z"/>
                        </svg>
                      </div>
                      <div class="flex-1 min-w-0">
                        <p class="font-medium text-gray-900">Microsoft Outlook</p>
                        <p class="text-sm text-gray-500">Outlook.com, Microsoft 365, Exchange</p>
                      </div>
                      <svg class="w-5 h-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                      </svg>
                    </button>

                    <!-- ICS URL -->
                    <button
                      type="button"
                      class="w-full flex items-center gap-4 p-4 border border-gray-200 rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-colors text-left"
                      (click)="modalStep.set('ics-url')"
                    >
                      <div class="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center flex-shrink-0">
                        <svg class="w-5 h-5 text-purple-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
                        </svg>
                      </div>
                      <div class="flex-1 min-w-0">
                        <p class="font-medium text-gray-900">Calendar URL (ICS)</p>
                        <p class="text-sm text-gray-500">iCloud, Fastmail, or any ICS/webcal link</p>
                      </div>
                      <svg class="w-5 h-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                      </svg>
                    </button>
                  </div>
                </div>
              }

              <!-- OAuth Loading -->
              @if (modalStep() === 'oauth-loading') {
                <div class="p-6 text-center">
                  <app-spinner size="lg" />
                  <p class="mt-4 text-gray-900 font-medium">Connecting to {{ calendarService.getProviderDisplayName(selectedProvider()!) }}...</p>
                  <p class="mt-2 text-sm text-gray-500">A new window will open for authorization.</p>
                  <app-button variant="ghost" class="mt-6" (onClick)="closeConnectModal()">
                    Cancel
                  </app-button>
                </div>
              }

              <!-- Popup Blocked -->
              @if (modalStep() === 'popup-blocked') {
                <div class="p-6">
                  <div class="flex items-center justify-between mb-4">
                    <h3 class="text-lg font-semibold text-gray-900">Authorization Window Blocked</h3>
                    <button
                      type="button"
                      class="p-1 text-gray-400 hover:text-gray-600 rounded-lg"
                      (click)="closeConnectModal()"
                    >
                      <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>

                  <div class="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-4">
                    <div class="flex gap-3">
                      <svg class="w-5 h-5 text-amber-600 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                      </svg>
                      <div>
                        <p class="text-sm text-amber-800 font-medium">Pop-up window was blocked</p>
                        <p class="text-sm text-amber-700 mt-1">
                          @if (isStandaloneMode()) {
                            This app is running in standalone mode (home screen) which doesn't support pop-up windows.
                          } @else {
                            Your browser blocked the authorization window. This is a security feature to protect you.
                          }
                        </p>
                      </div>
                    </div>
                  </div>

                  <p class="text-sm text-gray-600 mb-4">Choose how you'd like to continue:</p>

                  <div class="space-y-3">
                    <!-- Option 1: Open in new tab (if not standalone) -->
                    @if (!isStandaloneMode()) {
                      <button
                        type="button"
                        class="w-full flex items-center gap-3 p-4 border border-gray-200 rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-colors text-left"
                        (click)="openOAuthInNewTab()"
                      >
                        <div class="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center flex-shrink-0">
                          <svg class="w-5 h-5 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                          </svg>
                        </div>
                        <div class="flex-1">
                          <p class="font-medium text-gray-900">Open in new tab</p>
                          <p class="text-sm text-gray-500">Opens authorization in a new browser tab</p>
                        </div>
                      </button>
                    }

                    <!-- Option 2: Redirect in same window -->
                    <button
                      type="button"
                      class="w-full flex items-center gap-3 p-4 border border-gray-200 rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-colors text-left"
                      (click)="redirectToOAuth()"
                    >
                      <div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center flex-shrink-0">
                        <svg class="w-5 h-5 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                        </svg>
                      </div>
                      <div class="flex-1">
                        <p class="font-medium text-gray-900">Continue in this window</p>
                        <p class="text-sm text-gray-500">Redirects you to {{ calendarService.getProviderDisplayName(selectedProvider()!) }}</p>
                      </div>
                    </button>

                    <!-- Option 3: Copy link -->
                    <button
                      type="button"
                      class="w-full flex items-center gap-3 p-4 border border-gray-200 rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-colors text-left"
                      (click)="copyOAuthLink()"
                    >
                      <div class="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center flex-shrink-0">
                        <svg class="w-5 h-5 text-purple-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                        </svg>
                      </div>
                      <div class="flex-1">
                        <p class="font-medium text-gray-900">{{ linkCopied() ? 'Link copied!' : 'Copy authorization link' }}</p>
                        <p class="text-sm text-gray-500">Open the link manually in any browser</p>
                      </div>
                    </button>
                  </div>

                  <div class="mt-4 pt-4 border-t border-gray-100">
                    <app-button variant="secondary" class="w-full" (onClick)="modalStep.set('provider')">
                      Choose a Different Provider
                    </app-button>
                  </div>
                </div>
              }

              <!-- Select Calendars -->
              @if (modalStep() === 'select-calendars') {
                <div class="p-6">
                  <div class="flex items-center justify-between mb-4">
                    <div>
                      <h3 class="text-lg font-semibold text-gray-900">Select Calendars</h3>
                      <p class="text-sm text-gray-500">{{ calendarService.pendingAccountEmail() }}</p>
                    </div>
                    <button
                      type="button"
                      class="p-1 text-gray-400 hover:text-gray-600 rounded-lg"
                      (click)="closeConnectModal()"
                    >
                      <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>

                  <p class="text-sm text-gray-600 mb-4">
                    Found {{ calendarService.pendingCalendars().length }} calendar(s). Select which ones to sync:
                  </p>

                  <div class="space-y-2 max-h-64 overflow-y-auto mb-4">
                    @for (calendar of calendarService.pendingCalendars(); track calendar.id) {
                      <label
                        class="flex items-center gap-3 p-3 border rounded-lg cursor-pointer hover:bg-gray-50 transition-colors"
                        [class.border-primary-500]="isCalendarSelected(calendar.id)"
                        [class.bg-primary-50]="isCalendarSelected(calendar.id)"
                        [class.border-gray-200]="!isCalendarSelected(calendar.id)"
                      >
                        <input
                          type="checkbox"
                          [checked]="isCalendarSelected(calendar.id)"
                          (change)="toggleCalendarSelection(calendar)"
                          class="w-4 h-4 text-primary-600 rounded border-gray-300 focus:ring-primary-500"
                        />
                        <div
                          class="w-3 h-3 rounded-full flex-shrink-0"
                          [style.background-color]="calendar.color || '#6B7280'"
                        ></div>
                        <div class="flex-1 min-w-0">
                          <p class="text-sm font-medium text-gray-900 truncate">{{ calendar.name }}</p>
                          @if (calendar.isPrimary) {
                            <span class="text-xs text-gray-500">Primary</span>
                          }
                        </div>
                      </label>
                    }
                  </div>

                  <div class="flex items-center justify-between border-t border-gray-100 pt-4">
                    <button
                      type="button"
                      class="text-sm text-gray-600 hover:text-gray-900"
                      (click)="toggleSelectAll()"
                    >
                      {{ allCalendarsSelected() ? 'Deselect All' : 'Select All' }}
                    </button>
                    <span class="text-sm text-gray-500">{{ selectedCalendars().length }} selected</span>
                  </div>

                  <div class="flex justify-end gap-3 mt-6">
                    <app-button variant="secondary" (onClick)="modalStep.set('provider')">
                      Back
                    </app-button>
                    <app-button
                      variant="primary"
                      [disabled]="selectedCalendars().length === 0"
                      (onClick)="modalStep.set('assign-members')"
                    >
                      Continue
                    </app-button>
                  </div>
                </div>
              }

              <!-- Assign Members -->
              @if (modalStep() === 'assign-members') {
                <div class="p-6">
                  <div class="flex items-center justify-between mb-4">
                    <h3 class="text-lg font-semibold text-gray-900">Assign to Family Members</h3>
                    <button
                      type="button"
                      class="p-1 text-gray-400 hover:text-gray-600 rounded-lg"
                      (click)="closeConnectModal()"
                    >
                      <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>

                  <p class="text-sm text-gray-600 mb-4">
                    Who should see events from these calendars on the display?
                  </p>

                  <div class="space-y-4 max-h-80 overflow-y-auto mb-4">
                    @for (calendar of selectedCalendars(); track calendar.id) {
                      <div class="p-4 border border-gray-200 rounded-lg">
                        <div class="flex items-center gap-2 mb-3">
                          <div
                            class="w-3 h-3 rounded-full"
                            [style.background-color]="calendar.color || '#6B7280'"
                          ></div>
                          <span class="font-medium text-gray-900">{{ calendar.name }}</span>
                        </div>
                        <div class="flex flex-wrap gap-2">
                          @for (member of familyMembers(); track member.id) {
                            <button
                              type="button"
                              class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm border transition-colors"
                              [class.bg-primary-100]="isMemberAssigned(calendar.id, member.id)"
                              [class.border-primary-300]="isMemberAssigned(calendar.id, member.id)"
                              [class.text-primary-700]="isMemberAssigned(calendar.id, member.id)"
                              [class.bg-white]="!isMemberAssigned(calendar.id, member.id)"
                              [class.border-gray-300]="!isMemberAssigned(calendar.id, member.id)"
                              [class.text-gray-700]="!isMemberAssigned(calendar.id, member.id)"
                              [class.hover:bg-gray-50]="!isMemberAssigned(calendar.id, member.id)"
                              (click)="toggleMemberAssignment(calendar.id, member.id)"
                            >
                              <app-avatar
                                [name]="member.displayName"
                                [color]="member.profile.color"
                                size="xs"
                              />
                              {{ member.displayName }}
                              @if (isMemberAssigned(calendar.id, member.id)) {
                                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                                </svg>
                              }
                            </button>
                          }
                        </div>
                      </div>
                    }
                  </div>

                  <div class="flex justify-end gap-3 mt-6">
                    <app-button variant="secondary" (onClick)="modalStep.set('select-calendars')">
                      Back
                    </app-button>
                    <app-button
                      variant="primary"
                      [loading]="creating()"
                      (onClick)="createConnections()"
                    >
                      Connect {{ selectedCalendars().length }} Calendar{{ selectedCalendars().length > 1 ? 's' : '' }}
                    </app-button>
                  </div>
                </div>
              }

              <!-- ICS URL Input -->
              @if (modalStep() === 'ics-url') {
                <div class="p-6">
                  <div class="flex items-center justify-between mb-4">
                    <h3 class="text-lg font-semibold text-gray-900">Add Calendar URL</h3>
                    <button
                      type="button"
                      class="p-1 text-gray-400 hover:text-gray-600 rounded-lg"
                      (click)="closeConnectModal()"
                    >
                      <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>

                  <div class="space-y-4">
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-1">
                        Calendar URL
                      </label>
                      <input
                        type="url"
                        [(ngModel)]="icsUrl"
                        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                        placeholder="https://calendar.google.com/calendar/ical/..."
                      />
                      <p class="text-xs text-gray-500 mt-1">
                        Paste your calendar's ICS or webcal link
                      </p>
                    </div>

                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-1">
                        Calendar Name
                      </label>
                      <input
                        type="text"
                        [(ngModel)]="icsCalendarName"
                        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                        placeholder="e.g., Soccer Practice"
                      />
                    </div>

                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-2">
                        Assign to Members
                      </label>
                      <div class="flex flex-wrap gap-2">
                        @for (member of familyMembers(); track member.id) {
                          <button
                            type="button"
                            class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm border transition-colors"
                            [class.bg-primary-100]="icsMemberIds().includes(member.id)"
                            [class.border-primary-300]="icsMemberIds().includes(member.id)"
                            [class.text-primary-700]="icsMemberIds().includes(member.id)"
                            [class.bg-white]="!icsMemberIds().includes(member.id)"
                            [class.border-gray-300]="!icsMemberIds().includes(member.id)"
                            [class.text-gray-700]="!icsMemberIds().includes(member.id)"
                            (click)="toggleIcsMember(member.id)"
                          >
                            <app-avatar [name]="member.displayName" [color]="member.profile.color" size="xs" />
                            {{ member.displayName }}
                          </button>
                        }
                      </div>
                    </div>

                    @if (icsError()) {
                      <div class="p-3 bg-red-50 border border-red-200 rounded-lg">
                        <p class="text-sm text-red-700">{{ icsError() }}</p>
                      </div>
                    }
                  </div>

                  <div class="flex justify-end gap-3 mt-6">
                    <app-button variant="secondary" (onClick)="modalStep.set('provider')">
                      Back
                    </app-button>
                    <app-button
                      variant="primary"
                      [loading]="validatingIcs() || creating()"
                      [disabled]="!icsUrl || !icsCalendarName"
                      (onClick)="createIcsConnection()"
                    >
                      Connect Calendar
                    </app-button>
                  </div>
                </div>
              }

              <!-- Success -->
              @if (modalStep() === 'success') {
                <div class="p-6 text-center">
                  <div class="mx-auto w-12 h-12 bg-green-100 rounded-full flex items-center justify-center mb-4">
                    <svg class="w-6 h-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                    </svg>
                  </div>
                  <h3 class="text-lg font-semibold text-gray-900 mb-2">Calendars Connected!</h3>
                  <p class="text-sm text-gray-600 mb-6">
                    Your calendars are now syncing. Events will appear on the family display shortly.
                  </p>
                  <app-button variant="primary" (onClick)="closeConnectModal()">
                    Done
                  </app-button>
                </div>
              }
            </div>
          </div>
        </div>
      }

      <!-- Edit Calendar Modal -->
      @if (showEditModal()) {
        <div class="fixed inset-0 z-50 overflow-y-auto" (click)="closeEditModal()">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="modal-overlay"></div>
            <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6" (click)="$event.stopPropagation()">
              <div class="flex items-center justify-between mb-4">
                <h3 class="text-lg font-semibold text-gray-900">Calendar Settings</h3>
                <button
                  type="button"
                  class="p-1 text-gray-400 hover:text-gray-600 rounded-lg"
                  (click)="closeEditModal()"
                >
                  <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>

              <div class="space-y-4">
                <!-- Calendar Info -->
                <div class="flex items-center gap-3 p-3 bg-gray-50 rounded-lg">
                  <div
                    class="w-10 h-10 rounded-lg flex items-center justify-center"
                    [class]="getProviderBgColor(selectedConnection()!.provider)"
                  >
                    <span [innerHTML]="getProviderIcon(selectedConnection()!.provider)" class="w-5 h-5"></span>
                  </div>
                  <div>
                    <p class="font-medium text-gray-900">{{ selectedConnection()!.name }}</p>
                    <p class="text-sm text-gray-500">{{ selectedConnection()!.externalAccountId || calendarService.getProviderDisplayName(selectedConnection()!.provider) }}</p>
                  </div>
                </div>

                <!-- Display Name -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Display Name</label>
                  <input
                    type="text"
                    [(ngModel)]="editDisplayName"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  />
                </div>

                <!-- Assigned Members -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Assigned Members</label>
                  <div class="flex flex-wrap gap-2">
                    @for (member of familyMembers(); track member.id) {
                      <button
                        type="button"
                        class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm border transition-colors"
                        [class.bg-primary-100]="editMemberIds().includes(member.id)"
                        [class.border-primary-300]="editMemberIds().includes(member.id)"
                        [class.text-primary-700]="editMemberIds().includes(member.id)"
                        [class.bg-white]="!editMemberIds().includes(member.id)"
                        [class.border-gray-300]="!editMemberIds().includes(member.id)"
                        [class.text-gray-700]="!editMemberIds().includes(member.id)"
                        (click)="toggleEditMember(member.id)"
                      >
                        <app-avatar [name]="member.displayName" [color]="member.profile.color" size="xs" />
                        {{ member.displayName }}
                      </button>
                    }
                  </div>
                </div>

                <!-- Sync Settings -->
                <div class="border-t border-gray-100 pt-4">
                  <p class="text-sm font-medium text-gray-700 mb-3">Sync Settings</p>
                  <div class="grid grid-cols-2 gap-4">
                    <div>
                      <label class="block text-xs text-gray-500 mb-1">Past events</label>
                      <select
                        [(ngModel)]="editSyncPastDays"
                        class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                      >
                        <option [value]="7">7 days</option>
                        <option [value]="14">14 days</option>
                        <option [value]="30">30 days</option>
                        <option [value]="60">60 days</option>
                        <option [value]="90">90 days</option>
                      </select>
                    </div>
                    <div>
                      <label class="block text-xs text-gray-500 mb-1">Future events</label>
                      <select
                        [(ngModel)]="editSyncFutureDays"
                        class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                      >
                        <option [value]="30">30 days</option>
                        <option [value]="60">60 days</option>
                        <option [value]="90">90 days</option>
                        <option [value]="180">180 days</option>
                        <option [value]="365">1 year</option>
                      </select>
                    </div>
                  </div>
                </div>

                <!-- Last Sync Info -->
                @if (selectedConnection()!.lastSyncedAt) {
                  <div class="text-sm text-gray-500">
                    Last synced: {{ formatLastSync(selectedConnection()!.lastSyncedAt!) }}
                  </div>
                }
              </div>

              <div class="flex justify-end gap-3 mt-6">
                <app-button variant="secondary" (onClick)="closeEditModal()">
                  Cancel
                </app-button>
                <app-button
                  variant="primary"
                  [loading]="updating()"
                  (onClick)="updateConnection()"
                >
                  Save Changes
                </app-button>
              </div>
            </div>
          </div>
        </div>
      }

      <!-- Disconnect Confirmation Modal -->
      @if (showDisconnectModal()) {
        <div class="fixed inset-0 z-50 overflow-y-auto" (click)="closeDisconnectModal()">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="modal-overlay"></div>
            <div class="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6" (click)="$event.stopPropagation()">
              <h3 class="text-lg font-semibold text-gray-900 mb-2">Disconnect Calendar?</h3>
              <p class="text-sm text-gray-600 mb-4">
                Are you sure you want to disconnect <strong>{{ selectedConnection()?.name }}</strong>?
                Events from this calendar will no longer appear on the family display.
              </p>
              <p class="text-sm text-gray-500 mb-6">
                This won't delete events from your {{ calendarService.getProviderDisplayName(selectedConnection()!.provider) }} account.
              </p>

              <div class="flex justify-end gap-3">
                <app-button variant="secondary" (onClick)="closeDisconnectModal()">
                  Cancel
                </app-button>
                <app-button
                  variant="danger"
                  [loading]="disconnecting()"
                  (onClick)="disconnectCalendar()"
                >
                  Disconnect
                </app-button>
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `,
})
export class CalendarsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  readonly calendarService = inject(CalendarConnectionService);
  private readonly userService = inject(UserService);
  private readonly toastService = inject(ToastService);

  // Expose enums to template
  CalendarProvider = CalendarProvider;
  CalendarConnectionStatus = CalendarConnectionStatus;

  // State
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  openMenuId = signal<string | null>(null);

  // Connect Modal State
  showConnectModal = signal(false);
  modalStep = signal<ModalStep>('provider');
  selectedProvider = signal<CalendarProvider | null>(null);
  selectedCalendars = signal<ExternalCalendarInfo[]>([]);
  calendarMemberAssignments = signal<Map<string, string[]>>(new Map());
  creating = signal(false);

  // ICS State
  icsUrl = '';
  icsCalendarName = '';
  icsMemberIds = signal<string[]>([]);
  icsError = signal<string | null>(null);
  validatingIcs = signal(false);

  // Edit Modal State
  showEditModal = signal(false);
  selectedConnection = signal<CalendarConnection | null>(null);
  editDisplayName = '';
  editMemberIds = signal<string[]>([]);
  editSyncPastDays = 30;
  editSyncFutureDays = 90;
  updating = signal(false);

  // Disconnect Modal State
  showDisconnectModal = signal(false);
  disconnecting = signal(false);

  // OAuth popup reference
  private oauthPopup: Window | null = null;

  // Pending OAuth URL for fallback flows (popup blocked, PWA mode, etc.)
  private pendingOAuthUrl: string | null = null;
  linkCopied = signal(false);

  // Bound event handlers (stored for proper cleanup)
  private boundHandleOAuthCallback = this.handleOAuthCallback.bind(this);
  private boundHandleDocumentClick = this.handleDocumentClick.bind(this);

  // Computed
  connections = this.calendarService.connections;
  familyMembers = signal<User[]>([]);

  allCalendarsSelected = computed(() => {
    const pending = this.calendarService.pendingCalendars();
    const selected = this.selectedCalendars();
    return pending.length > 0 && pending.length === selected.length;
  });

  canManageCalendars = () => {
    const role = this.authService.user()?.role;
    return role === 'Owner' || role === 'Admin' || role === 'Adult';
  };

  ngOnInit(): void {
    const familyId = this.authService.user()?.familyId;
    if (familyId) {
      this.calendarService.getConnections(familyId).subscribe();
      this.loadFamilyMembers(familyId);
    }

    // Listen for OAuth callback messages
    window.addEventListener('message', this.boundHandleOAuthCallback);

    // Close menu when clicking outside
    document.addEventListener('click', this.boundHandleDocumentClick);
  }

  ngOnDestroy(): void {
    window.removeEventListener('message', this.boundHandleOAuthCallback);
    document.removeEventListener('click', this.boundHandleDocumentClick);
  }

  private handleDocumentClick(event: MouseEvent): void {
    // Close menu if clicking outside of it
    if (this.openMenuId() !== null) {
      const target = event.target as HTMLElement;
      const menuButton = target.closest('[data-menu-button]');
      const menuDropdown = target.closest('[data-menu-dropdown]');
      if (!menuButton && !menuDropdown) {
        this.openMenuId.set(null);
      }
    }
  }

  private loadFamilyMembers(familyId: string): void {
    this.userService.getFamilyMembers(familyId).subscribe({
      next: (members) => this.familyMembers.set(members),
      error: () => console.error('Failed to load family members'),
    });
  }

  // Provider icons and colors
  getProviderBgColor(provider: CalendarProvider): string {
    const colors: Partial<Record<CalendarProvider, string>> = {
      [CalendarProvider.Google]: 'bg-red-100 text-red-600',
      [CalendarProvider.Outlook]: 'bg-blue-100 text-blue-600',
      [CalendarProvider.ICloud]: 'bg-gray-100 text-gray-600',
      [CalendarProvider.CalDav]: 'bg-green-100 text-green-600',
      [CalendarProvider.IcsUrl]: 'bg-purple-100 text-purple-600',
      [CalendarProvider.Internal]: 'bg-primary-100 text-primary-600',
    };
    return colors[provider] || 'bg-gray-100 text-gray-600';
  }

  getProviderIcon(provider: CalendarProvider): string {
    const icons: Partial<Record<CalendarProvider, string>> = {
      [CalendarProvider.Google]: `<svg viewBox="0 0 24 24" fill="currentColor">
        <path d="M12.545,10.239v3.821h5.445c-0.712,2.315-2.647,3.972-5.445,3.972c-3.332,0-6.033-2.701-6.033-6.032s2.701-6.032,6.033-6.032c1.498,0,2.866,0.549,3.921,1.453l2.814-2.814C17.503,2.988,15.139,2,12.545,2C7.021,2,2.543,6.477,2.543,12s4.478,10,10.002,10c8.396,0,10.249-7.85,9.426-11.748L12.545,10.239z"/>
      </svg>`,
      [CalendarProvider.Outlook]: `<svg viewBox="0 0 24 24" fill="currentColor">
        <path d="M11.5,3v8.5H3V3H11.5z M11.5,21H3v-8.5h8.5V21z M12.5,3H21v8.5h-8.5V3z M21,12.5V21h-8.5v-8.5H21z"/>
      </svg>`,
      [CalendarProvider.ICloud]: `<svg viewBox="0 0 24 24" fill="currentColor">
        <path d="M18.71,19.5C17.88,20.74 17,21.95 15.66,21.97C14.32,22 13.89,21.18 12.37,21.18C10.84,21.18 10.37,21.95 9.1,22C7.79,22.05 6.8,20.68 5.96,19.47C4.25,17 2.94,12.45 4.7,9.39C5.57,7.87 7.13,6.91 8.82,6.88C10.1,6.86 11.32,7.75 12.11,7.75C12.89,7.75 14.37,6.68 15.92,6.84C16.57,6.87 18.39,7.1 19.56,8.82C19.47,8.88 17.39,10.1 17.41,12.63C17.44,15.65 20.06,16.66 20.09,16.67C20.06,16.74 19.67,18.11 18.71,19.5M13,3.5C13.73,2.67 14.94,2.04 15.94,2C16.07,3.17 15.6,4.35 14.9,5.19C14.21,6.04 13.07,6.7 11.95,6.61C11.8,5.46 12.36,4.26 13,3.5Z"/>
      </svg>`,
      [CalendarProvider.CalDav]: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
      </svg>`,
      [CalendarProvider.IcsUrl]: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
      </svg>`,
      [CalendarProvider.Internal]: `<svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>`,
    };
    return icons[provider] || '';
  }

  // Menu handling
  toggleMenu(connectionId: string): void {
    this.openMenuId.update((id) => (id === connectionId ? null : connectionId));
  }

  // Member helpers
  getAssignedMembers(connection: CalendarConnection): User[] {
    return this.familyMembers().filter((m) =>
      connection.assignedMemberIds?.includes(m.id)
    );
  }

  getAssignedMemberNames(connection: CalendarConnection): string {
    const members = this.getAssignedMembers(connection);
    if (members.length === 0) return '';
    if (members.length === 1) return members[0].displayName;
    if (members.length === 2) return `${members[0].displayName} & ${members[1].displayName}`;
    return `${members[0].displayName} +${members.length - 1} more`;
  }

  // Date formatting
  formatLastSync(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString();
  }

  // Connect Modal
  openConnectModal(): void {
    this.showConnectModal.set(true);
    this.modalStep.set('provider');
    this.selectedProvider.set(null);
    this.selectedCalendars.set([]);
    this.calendarMemberAssignments.set(new Map());
    this.icsUrl = '';
    this.icsCalendarName = '';
    this.icsMemberIds.set([]);
    this.icsError.set(null);
    this.calendarService.clearPendingOAuth();
  }

  closeConnectModal(): void {
    this.showConnectModal.set(false);
    this.openMenuId.set(null);
    if (this.oauthPopup && !this.oauthPopup.closed) {
      this.oauthPopup.close();
    }
  }

  // OAuth Flow
  startOAuthFlow(provider: CalendarProvider): void {
    const familyId = this.authService.user()?.familyId;
    if (!familyId) return;

    this.selectedProvider.set(provider);
    this.modalStep.set('oauth-loading');
    this.linkCopied.set(false);

    const redirectUri = `${window.location.origin}/oauth/callback`;

    this.calendarService.startOAuth(familyId, provider, redirectUri).subscribe({
      next: (response) => {
        // Store URL for fallback flows
        this.pendingOAuthUrl = response.authorizationUrl;

        // Check for standalone mode (PWA / home screen app) - popups don't work
        if (this.isStandaloneMode()) {
          this.modalStep.set('popup-blocked');
          return;
        }

        // Open OAuth popup
        const width = 600;
        const height = 700;
        const left = window.screenX + (window.outerWidth - width) / 2;
        const top = window.screenY + (window.outerHeight - height) / 2;

        this.oauthPopup = window.open(
          response.authorizationUrl,
          'oauth_popup',
          `width=${width},height=${height},left=${left},top=${top},toolbar=no,menubar=no`
        );

        // Check if popup was blocked
        if (!this.oauthPopup || this.oauthPopup.closed) {
          this.modalStep.set('popup-blocked');
          return;
        }

        // Poll for popup close
        const pollTimer = setInterval(() => {
          if (this.oauthPopup?.closed) {
            clearInterval(pollTimer);
            if (this.modalStep() === 'oauth-loading') {
              // User closed popup without completing
              this.modalStep.set('provider');
            }
          }
        }, 500);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to start authorization');
        this.modalStep.set('provider');
      },
    });
  }

  /**
   * Detects if the app is running in standalone mode (PWA / added to home screen).
   * In standalone mode, window.open() either doesn't work or navigates the current page.
   */
  isStandaloneMode(): boolean {
    // Check for iOS standalone mode
    const isIOSStandalone = ('standalone' in window.navigator) && (window.navigator as { standalone?: boolean }).standalone === true;

    // Check for PWA display-mode
    const isPWA = window.matchMedia('(display-mode: standalone)').matches
                  || window.matchMedia('(display-mode: fullscreen)').matches;

    return isIOSStandalone || isPWA;
  }

  /**
   * Opens the OAuth URL in a new tab (fallback when popup is blocked).
   */
  openOAuthInNewTab(): void {
    if (!this.pendingOAuthUrl) return;

    // Use window.open with _blank target which is less likely to be blocked
    const newWindow = window.open(this.pendingOAuthUrl, '_blank');

    if (newWindow) {
      this.modalStep.set('oauth-loading');
    } else {
      // Still blocked, keep showing the popup-blocked modal
      this.toastService.error('Unable to open new tab. Please try the redirect option or copy the link.');
    }
  }

  /**
   * Redirects the current page to the OAuth provider.
   * The user will be redirected back to /oauth/callback after authorization.
   * The familyId is extracted server-side from the OAuth session (tied to the state parameter).
   */
  redirectToOAuth(): void {
    if (!this.pendingOAuthUrl) return;

    // Store marker in sessionStorage so callback knows this is a redirect flow
    sessionStorage.setItem('luminous_oauth_redirect', 'true');

    // Redirect to OAuth provider
    window.location.href = this.pendingOAuthUrl;
  }

  /**
   * Copies the OAuth URL to clipboard for manual opening.
   */
  async copyOAuthLink(): Promise<void> {
    if (!this.pendingOAuthUrl) return;

    try {
      await navigator.clipboard.writeText(this.pendingOAuthUrl);
      this.linkCopied.set(true);
      this.toastService.success('Link copied to clipboard');

      // Reset after 3 seconds
      setTimeout(() => this.linkCopied.set(false), 3000);
    } catch {
      this.toastService.error('Failed to copy link');
    }
  }

  private handleOAuthCallback(event: MessageEvent): void {
    if (event.origin !== window.location.origin) return;
    if (event.data?.type !== 'oauth_callback') return;

    const { code, state, error } = event.data;

    if (error) {
      this.error.set(error === 'access_denied' ? 'Authorization was cancelled' : error);
      this.modalStep.set('provider');
      return;
    }

    if (!code || !state) return;

    const redirectUri = `${window.location.origin}/oauth/callback`;

    // Use completeOAuthByState which extracts familyId from the server-side session.
    // The state token cryptographically proves ownership of the session.
    this.calendarService
      .completeOAuthByState({ code, state, redirectUri })
      .subscribe({
        next: () => {
          this.modalStep.set('select-calendars');
          // Pre-select primary calendar
          const primary = this.calendarService.pendingCalendars().find((c) => c.isPrimary);
          if (primary) {
            this.selectedCalendars.set([primary]);
          }
        },
        error: (err) => {
          this.error.set(err.message || 'Failed to complete authorization');
          this.modalStep.set('provider');
        },
      });
  }

  // Calendar Selection
  isCalendarSelected(calendarId: string): boolean {
    return this.selectedCalendars().some((c) => c.id === calendarId);
  }

  toggleCalendarSelection(calendar: ExternalCalendarInfo): void {
    this.selectedCalendars.update((selected) => {
      const exists = selected.some((c) => c.id === calendar.id);
      if (exists) {
        return selected.filter((c) => c.id !== calendar.id);
      }
      return [...selected, calendar];
    });
  }

  toggleSelectAll(): void {
    if (this.allCalendarsSelected()) {
      this.selectedCalendars.set([]);
    } else {
      this.selectedCalendars.set([...this.calendarService.pendingCalendars()]);
    }
  }

  // Member Assignment
  isMemberAssigned(calendarId: string, memberId: string): boolean {
    const assignments = this.calendarMemberAssignments();
    return assignments.get(calendarId)?.includes(memberId) || false;
  }

  toggleMemberAssignment(calendarId: string, memberId: string): void {
    this.calendarMemberAssignments.update((map) => {
      const newMap = new Map(map);
      const current = newMap.get(calendarId) || [];
      if (current.includes(memberId)) {
        newMap.set(calendarId, current.filter((id) => id !== memberId));
      } else {
        newMap.set(calendarId, [...current, memberId]);
      }
      return newMap;
    });
  }

  // Create Connections
  createConnections(): void {
    const familyId = this.authService.user()?.familyId;
    const sessionId = this.calendarService.pendingSessionId();
    if (!familyId || !sessionId) return;

    this.creating.set(true);

    const calendars = this.selectedCalendars();
    const assignments = this.calendarMemberAssignments();

    // Create all connections at once using the session-based API
    const request = {
      sessionId,
      calendars: calendars.map((calendar) => ({
        externalCalendarId: calendar.id,
        displayName: calendar.name,
        color: calendar.color,
        assignedMemberIds: assignments.get(calendar.id) || [],
      })),
    };

    this.calendarService
      .createConnectionsFromSession(familyId, request)
      .subscribe({
        next: () => {
          this.creating.set(false);
          this.modalStep.set('success');
          this.calendarService.clearPendingOAuth();
        },
        error: (err) => {
          this.creating.set(false);
          this.error.set(err.message || 'Failed to connect calendars');
        },
      });
  }

  // ICS Connection
  toggleIcsMember(memberId: string): void {
    this.icsMemberIds.update((ids) => {
      if (ids.includes(memberId)) {
        return ids.filter((id) => id !== memberId);
      }
      return [...ids, memberId];
    });
  }

  createIcsConnection(): void {
    const familyId = this.authService.user()?.familyId;
    if (!familyId || !this.icsUrl || !this.icsCalendarName) return;

    this.validatingIcs.set(true);
    this.icsError.set(null);

    // Convert webcal:// to https://
    let url = this.icsUrl.trim();
    if (url.startsWith('webcal://')) {
      url = url.replace('webcal://', 'https://');
    }

    // First validate the URL
    this.calendarService.validateIcsUrl(familyId, url).subscribe({
      next: (result) => {
        this.validatingIcs.set(false);
        if (!result.isValid) {
          this.icsError.set(result.error || 'Invalid calendar URL');
          return;
        }

        // Create the connection
        this.creating.set(true);
        this.calendarService
          .createConnection(familyId, {
            provider: CalendarProvider.IcsUrl,
            name: this.icsCalendarName,
            icsUrl: url,
            assignedMemberIds: this.icsMemberIds(),
          })
          .subscribe({
            next: () => {
              this.creating.set(false);
              this.modalStep.set('success');
            },
            error: (err) => {
              this.creating.set(false);
              this.icsError.set(err.message || 'Failed to connect calendar');
            },
          });
      },
      error: (err) => {
        this.validatingIcs.set(false);
        this.icsError.set(err.message || 'Failed to validate calendar URL');
      },
    });
  }

  // Sync Calendar
  syncCalendar(connection: CalendarConnection): void {
    this.openMenuId.set(null);
    const familyId = this.authService.user()?.familyId;
    if (!familyId) return;

    this.calendarService.syncConnection(familyId, connection.id).subscribe({
      next: (result) => {
        this.toastService.success(`Synced ${result.eventsAdded + result.eventsUpdated} events`);
      },
      error: (err) => {
        this.toastService.error(err.message || 'Sync failed');
      },
    });
  }

  // Reconnect (re-auth)
  reconnectCalendar(connection: CalendarConnection): void {
    if (connection.provider === CalendarProvider.IcsUrl) {
      this.openEditModal(connection);
    } else {
      this.selectedProvider.set(connection.provider);
      this.showConnectModal.set(true);
      this.startOAuthFlow(connection.provider);
    }
  }

  // Edit Modal
  openEditModal(connection: CalendarConnection): void {
    this.openMenuId.set(null);
    this.selectedConnection.set(connection);
    this.editDisplayName = connection.name;
    this.editMemberIds.set([...(connection.assignedMemberIds ?? [])]);
    this.editSyncPastDays = connection.syncSettings.syncPastDays;
    this.editSyncFutureDays = connection.syncSettings.syncFutureDays;
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.selectedConnection.set(null);
  }

  toggleEditMember(memberId: string): void {
    this.editMemberIds.update((ids) => {
      if (ids.includes(memberId)) {
        return ids.filter((id) => id !== memberId);
      }
      return [...ids, memberId];
    });
  }

  updateConnection(): void {
    const familyId = this.authService.user()?.familyId;
    const connection = this.selectedConnection();
    if (!familyId || !connection) return;

    this.updating.set(true);

    this.calendarService
      .updateConnection(familyId, connection.id, {
        name: this.editDisplayName,
        assignedMemberIds: this.editMemberIds(),
        syncSettings: {
          syncPastDays: this.editSyncPastDays,
          syncFutureDays: this.editSyncFutureDays,
        },
      })
      .subscribe({
        next: () => {
          this.updating.set(false);
          this.closeEditModal();
          this.successMessage.set('Calendar updated successfully');
        },
        error: (err) => {
          this.updating.set(false);
          this.error.set(err.message || 'Failed to update calendar');
        },
      });
  }

  // Disconnect Modal
  confirmDisconnect(connection: CalendarConnection): void {
    this.openMenuId.set(null);
    this.selectedConnection.set(connection);
    this.showDisconnectModal.set(true);
  }

  closeDisconnectModal(): void {
    this.showDisconnectModal.set(false);
    this.selectedConnection.set(null);
  }

  disconnectCalendar(): void {
    const familyId = this.authService.user()?.familyId;
    const connection = this.selectedConnection();
    if (!familyId || !connection) return;

    this.disconnecting.set(true);

    this.calendarService.deleteConnection(familyId, connection.id).subscribe({
      next: () => {
        this.disconnecting.set(false);
        this.closeDisconnectModal();
        this.successMessage.set('Calendar disconnected');
      },
      error: (err) => {
        this.disconnecting.set(false);
        this.error.set(err.message || 'Failed to disconnect calendar');
      },
    });
  }

  // Error handling
  clearError(): void {
    this.error.set(null);
    this.calendarService.clearError();
  }
}

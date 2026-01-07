import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MemberData } from '../../../../core/services/cache.service';

@Component({
  selector: 'app-profile-filter',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="profile-filter">
      <div class="filter-header">
        <span class="filter-label">Filter by family member</span>
        @if (selectedMemberIds.length > 0) {
          <button class="clear-btn" (click)="clearFilter()">
            Clear
          </button>
        }
      </div>

      <div class="member-list">
        <!-- Show all option -->
        <button
          class="member-chip all"
          [class.active]="selectedMemberIds.length === 0"
          (click)="clearFilter()"
        >
          <div class="chip-avatar all-avatar">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/>
              <circle cx="9" cy="7" r="4"/>
              <path d="M22 21v-2a4 4 0 0 0-3-3.87"/>
              <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
            </svg>
          </div>
          <span class="chip-name">Everyone</span>
        </button>

        <!-- Individual member chips -->
        @for (member of members; track member.id) {
          <button
            class="member-chip"
            [class.active]="isSelected(member.id)"
            [style.--member-color]="member.color"
            (click)="toggleMember(member.id)"
          >
            <div class="chip-avatar" [style.background-color]="member.color">
              @if (member.avatarUrl) {
                <img [src]="member.avatarUrl" [alt]="member.name" />
              } @else {
                {{ member.initials }}
              }
            </div>
            <span class="chip-name">{{ member.name }}</span>
            @if (isSelected(member.id)) {
              <div class="chip-check">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="20 6 9 17 4 12"/>
                </svg>
              </div>
            }
          </button>
        }
      </div>

      @if (selectedMemberIds.length > 0) {
        <div class="filter-summary">
          Showing events for {{ getSelectedNames() }}
        </div>
      }
    </div>
  `,
  styles: [`
    .profile-filter {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
      padding: var(--space-4);
      background: var(--surface-primary);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-sm);
    }

    .filter-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .filter-label {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .clear-btn {
      padding: var(--space-1) var(--space-3);
      background: var(--surface-secondary);
      border: none;
      border-radius: var(--radius-full);
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-secondary);
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
    }

    .clear-btn:hover {
      background: var(--surface-pressed);
    }

    .clear-btn:active {
      transform: scale(0.95);
    }

    .member-list {
      display: flex;
      flex-wrap: wrap;
      gap: var(--space-2);
    }

    .member-chip {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-2) var(--space-3);
      background: var(--surface-secondary);
      border: 2px solid transparent;
      border-radius: var(--radius-full);
      cursor: pointer;
      transition: all var(--duration-quick) var(--ease-out);
      min-height: var(--touch-min);
    }

    .member-chip:hover {
      background: var(--surface-interactive);
    }

    .member-chip:active {
      transform: scale(0.98);
    }

    .member-chip.active {
      background: rgba(var(--member-color-rgb, 14, 165, 233), 0.1);
      border-color: var(--member-color, var(--accent-500));
    }

    .member-chip.all.active {
      background: var(--accent-100);
      border-color: var(--accent-500);
    }

    .chip-avatar {
      width: 32px;
      height: 32px;
      border-radius: var(--radius-full);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 12px;
      font-weight: 600;
      color: white;
      overflow: hidden;
    }

    .chip-avatar img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .chip-avatar.all-avatar {
      background: var(--surface-pressed);
      color: var(--text-secondary);
    }

    .member-chip.all.active .all-avatar {
      background: var(--accent-500);
      color: white;
    }

    .chip-name {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-primary);
    }

    .chip-check {
      width: 20px;
      height: 20px;
      background: var(--member-color, var(--accent-500));
      border-radius: var(--radius-full);
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
    }

    .filter-summary {
      font-size: 0.875rem;
      color: var(--text-secondary);
      padding-top: var(--space-2);
      border-top: 1px solid var(--border-color-light);
    }
  `],
})
export class ProfileFilterComponent {
  @Input() members: MemberData[] = [];
  @Input() selectedMemberIds: string[] = [];

  @Output() selectionChange = new EventEmitter<string[]>();

  isSelected(memberId: string): boolean {
    return this.selectedMemberIds.includes(memberId);
  }

  toggleMember(memberId: string): void {
    let newSelection: string[];

    if (this.isSelected(memberId)) {
      newSelection = this.selectedMemberIds.filter(id => id !== memberId);
    } else {
      newSelection = [...this.selectedMemberIds, memberId];
    }

    this.selectionChange.emit(newSelection);
  }

  clearFilter(): void {
    this.selectionChange.emit([]);
  }

  getSelectedNames(): string {
    const selectedMembers = this.members.filter(m => this.selectedMemberIds.includes(m.id));

    if (selectedMembers.length === 0) {
      return 'everyone';
    }

    if (selectedMembers.length === 1) {
      return selectedMembers[0].name;
    }

    if (selectedMembers.length === 2) {
      return `${selectedMembers[0].name} and ${selectedMembers[1].name}`;
    }

    const firstNames = selectedMembers.slice(0, -1).map(m => m.name).join(', ');
    const lastName = selectedMembers[selectedMembers.length - 1].name;
    return `${firstNames}, and ${lastName}`;
  }
}

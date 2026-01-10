import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskData, MemberData } from '../../../../core/services/cache.service';

@Component({
  selector: 'app-tasks-view',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="tasks-view">
      <div class="tasks-header">
        <h2 class="text-display-md">Today's Tasks</h2>
        @if (tasks.length > 0) {
          <div class="tasks-progress">
            <span class="text-glanceable">{{ completedCount }} / {{ tasks.length }}</span>
          </div>
        }
      </div>

      @if (isLoading) {
        <div class="tasks-loading">
          <div class="display-spinner display-spinner-lg"></div>
        </div>
      } @else if (tasks.length === 0) {
        <div class="display-empty">
          <div class="display-empty-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10z"/>
              <path d="m9 12 2 2 4-4"/>
            </svg>
          </div>
          <h3 class="display-empty-title">All done!</h3>
          <p class="display-empty-description">
            No tasks for today. Time to relax!
          </p>
        </div>
      } @else {
        <!-- Progress bar -->
        <div class="tasks-progress-bar">
          <div class="display-progress">
            <div
              class="display-progress-fill"
              [style.width.%]="progressPercent"
            ></div>
          </div>
        </div>

        <!-- Task list -->
        <div class="tasks-grid">
          @for (task of sortedTasks; track task.id) {
            <div
              class="display-task"
              [attr.data-completed]="task.completed"
              [style.--member-color]="getMemberColor(task.assigneeId)"
            >
              <div
                class="display-task-checkbox"
                [attr.data-checked]="task.completed"
              >
                @if (task.completed) {
                  <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
                    <polyline points="20 6 9 17 4 12"/>
                  </svg>
                }
              </div>
              <div class="task-content">
                <div class="display-task-title">{{ task.title }}</div>
                @if (task.assigneeId && getMember(task.assigneeId); as member) {
                  <div class="task-assignee">
                    <span
                      class="task-assignee-dot"
                      [style.background-color]="member.color"
                    ></span>
                    <span class="task-assignee-name">{{ member.name }}</span>
                  </div>
                }
              </div>
              @if (task.priority === 'high') {
                <div class="task-priority-badge">!</div>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .tasks-view {
      height: 100%;
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
      min-height: 0;
      overflow: hidden;
    }

    .tasks-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .tasks-progress {
      color: var(--text-secondary);
    }

    .tasks-loading {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .tasks-progress-bar {
      padding: 0 var(--space-4);
    }

    .tasks-grid {
      flex: 1;
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: var(--space-4);
      overflow-y: auto;
      padding: var(--space-4);
      min-height: 0;
    }

    .display-task {
      background: var(--surface-primary);
      border-radius: var(--radius-xl);
      padding: var(--space-5);
      box-shadow: var(--shadow-sm);
      display: flex;
      align-items: flex-start;
      gap: var(--space-4);
      transition: all var(--duration-quick) var(--ease-out);
    }

    .display-task:active {
      transform: scale(0.98);
    }

    .display-task[data-completed="true"] {
      opacity: 0.6;
    }

    .task-content {
      flex: 1;
    }

    .task-assignee {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      margin-top: var(--space-2);
      font-size: 0.875rem;
      color: var(--text-secondary);
    }

    .task-assignee-dot {
      width: 12px;
      height: 12px;
      border-radius: var(--radius-full);
    }

    .task-priority-badge {
      width: 28px;
      height: 28px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--danger);
      color: white;
      border-radius: var(--radius-full);
      font-weight: 700;
      font-size: 1rem;
    }
  `],
})
export class TasksViewComponent {
  @Input() tasks: TaskData[] = [];
  @Input() members: MemberData[] = [];
  @Input() isLoading = false;

  get completedCount(): number {
    return this.tasks.filter((t) => t.completed).length;
  }

  get progressPercent(): number {
    if (this.tasks.length === 0) return 0;
    return (this.completedCount / this.tasks.length) * 100;
  }

  get sortedTasks(): TaskData[] {
    return [...this.tasks].sort((a, b) => {
      // Incomplete tasks first
      if (a.completed !== b.completed) {
        return a.completed ? 1 : -1;
      }
      // High priority first
      if (a.priority !== b.priority) {
        const priorityOrder = { high: 0, medium: 1, low: 2 };
        return (priorityOrder[a.priority || 'low'] || 2) - (priorityOrder[b.priority || 'low'] || 2);
      }
      return 0;
    });
  }

  getMember(memberId: string | undefined): MemberData | undefined {
    if (!memberId) return undefined;
    return this.members.find((m) => m.id === memberId);
  }

  getMemberColor(memberId: string | undefined): string {
    const member = this.getMember(memberId);
    return member?.color || 'var(--accent-500)';
  }
}

import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface Feature {
  title: string;
  description: string;
  icon: string;
  color: string;
  colorLight: string;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css',
})
export class LandingComponent implements OnInit, OnDestroy {
  currentTime = signal('');
  currentDate = signal('');
  private timeInterval?: ReturnType<typeof setInterval>;

  // Animated orbs for visual interest
  orbs = [
    { size: 400, x: 10, y: 20, color: 'sky', delay: 0, duration: 25 },
    { size: 300, x: 70, y: 60, color: 'emerald', delay: 5, duration: 30 },
    { size: 350, x: 80, y: 10, color: 'violet', delay: 2, duration: 28 },
    { size: 250, x: 20, y: 70, color: 'rose', delay: 8, duration: 22 },
    { size: 200, x: 50, y: 40, color: 'amber', delay: 4, duration: 26 },
  ];

  features: Feature[] = [
    {
      title: 'Family Calendar',
      description: 'Unified view of everyone\'s schedules. Sync with Google, Outlook, and iCloud. See what\'s next at a glance.',
      icon: 'calendar',
      color: 'var(--member-sky)',
      colorLight: 'var(--member-sky-light)',
    },
    {
      title: 'Chores & Routines',
      description: 'Visual routines that kids can follow. Assign tasks, track completion, build healthy habits together.',
      icon: 'tasks',
      color: 'var(--member-emerald)',
      colorLight: 'var(--member-emerald-light)',
    },
    {
      title: 'Rewards System',
      description: 'Motivate with stars and rewards. Set goals, celebrate achievements, make responsibility fun.',
      icon: 'star',
      color: 'var(--member-amber)',
      colorLight: 'var(--member-amber-light)',
    },
    {
      title: 'Meal Planning',
      description: 'Plan the week\'s meals together. Save recipes, generate grocery lists, reduce dinner stress.',
      icon: 'meals',
      color: 'var(--member-orange)',
      colorLight: 'var(--member-orange-light)',
    },
    {
      title: 'Shared Lists',
      description: 'Groceries, packing, to-dos. Everyone can add, check off, and stay synced in real-time.',
      icon: 'lists',
      color: 'var(--member-rose)',
      colorLight: 'var(--member-rose-light)',
    },
    {
      title: 'Magic Import',
      description: 'Forward emails, scan schedules, snap photos. AI extracts events automatically for your approval.',
      icon: 'magic',
      color: 'var(--member-violet)',
      colorLight: 'var(--member-violet-light)',
    },
  ];

  testimonials = [
    {
      quote: 'Finally, everyone knows what\'s happening without asking me twenty times.',
      author: 'Sarah M.',
      role: 'Mom of 3',
    },
    {
      quote: 'My kids actually check their chores now. The reward system works!',
      author: 'Michael T.',
      role: 'Dad of 2',
    },
    {
      quote: 'The calm design is perfect. It fits in our kitchen like furniture.',
      author: 'Jamie L.',
      role: 'Parent & Designer',
    },
  ];

  ngOnInit(): void {
    this.updateTime();
    this.timeInterval = setInterval(() => this.updateTime(), 1000);
  }

  ngOnDestroy(): void {
    if (this.timeInterval) {
      clearInterval(this.timeInterval);
    }
  }

  private updateTime(): void {
    const now = new Date();
    this.currentTime.set(
      now.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit', hour12: true })
    );
    this.currentDate.set(
      now.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' })
    );
  }
}

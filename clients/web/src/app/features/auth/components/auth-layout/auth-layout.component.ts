import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  templateUrl: './auth-layout.component.html',
  styleUrl: './auth-layout.component.css',
})
export class AuthLayoutComponent implements OnInit, OnDestroy {
  currentTime = signal('');
  private timeInterval?: ReturnType<typeof setInterval>;

  // Floating elements for visual interest
  floatingItems = [
    { icon: 'calendar', x: 15, y: 25, delay: 0, color: 'sky' },
    { icon: 'check', x: 75, y: 15, delay: 1, color: 'emerald' },
    { icon: 'star', x: 85, y: 55, delay: 2, color: 'amber' },
    { icon: 'heart', x: 10, y: 70, delay: 3, color: 'rose' },
    { icon: 'users', x: 70, y: 80, delay: 4, color: 'violet' },
    { icon: 'list', x: 25, y: 45, delay: 5, color: 'teal' },
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
  }
}

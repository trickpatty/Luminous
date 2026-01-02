import { Component, inject, NgZone, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { CanvasService } from './core/services/canvas.service';
import { ElectronService } from './core/services/electron.service';
import { ExitDialogComponent } from './shared/components/exit-dialog/exit-dialog.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, ExitDialogComponent],
  template: `
    <div class="app-container" [attr.data-theme]="theme()">
      <router-outlet></router-outlet>
      @if (showExitDialog()) {
        <app-exit-dialog
          [externalError]="exitPinError()"
          (close)="closeExitDialog()"
          (exit)="exitApp($event)"
        />
      }
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      background-color: var(--canvas);
      transition: background-color var(--duration-slow) var(--ease-in-out);
    }
  `],
})
export class AppComponent implements OnInit, OnDestroy {
  private readonly canvasService = inject(CanvasService);
  private readonly electronService = inject(ElectronService);
  private readonly ngZone = inject(NgZone);

  protected readonly theme = this.canvasService.currentTheme;
  protected readonly showExitDialog = signal(false);
  protected readonly exitPinError = signal<string | null>(null);

  private exitDialogCleanup?: () => void;

  ngOnInit(): void {
    // Start canvas time-based adaptation
    this.canvasService.startAdaptation();

    // Listen for exit dialog trigger from Electron
    // Use NgZone.run() to ensure Angular change detection runs
    if (this.electronService.isElectron()) {
      this.exitDialogCleanup = this.electronService.onShowExitDialog(() => {
        this.ngZone.run(() => {
          this.exitPinError.set(null);
          this.showExitDialog.set(true);
        });
      });
    }
  }

  ngOnDestroy(): void {
    this.canvasService.stopAdaptation();
    this.exitDialogCleanup?.();
  }

  closeExitDialog(): void {
    this.showExitDialog.set(false);
    this.exitPinError.set(null);
  }

  async exitApp(pin: string): Promise<void> {
    this.exitPinError.set(null);

    if (this.electronService.isElectron()) {
      const success = await this.electronService.verifyExitPin(pin);
      if (!success) {
        // PIN incorrect - show error in dialog, don't close
        this.exitPinError.set('Incorrect PIN');
        return;
      }
      // If successful, the main process will quit the app
    }
    this.showExitDialog.set(false);
  }
}

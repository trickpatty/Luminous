import { Component, inject, OnInit, OnDestroy } from '@angular/core';
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
      @if (showExitDialog) {
        <app-exit-dialog
          (close)="showExitDialog = false"
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

  protected readonly theme = this.canvasService.currentTheme;
  protected showExitDialog = false;

  private exitDialogCleanup?: () => void;

  ngOnInit(): void {
    // Start canvas time-based adaptation
    this.canvasService.startAdaptation();

    // Listen for exit dialog trigger from Electron
    if (this.electronService.isElectron()) {
      this.exitDialogCleanup = this.electronService.onShowExitDialog(() => {
        this.showExitDialog = true;
      });
    }
  }

  ngOnDestroy(): void {
    this.canvasService.stopAdaptation();
    this.exitDialogCleanup?.();
  }

  exitApp(pin: string): void {
    if (this.electronService.isElectron()) {
      this.electronService.verifyExitPin(pin).then((success) => {
        if (!success) {
          // PIN incorrect - dialog will show error
          console.log('Exit PIN incorrect');
        }
      });
    }
    this.showExitDialog = false;
  }
}

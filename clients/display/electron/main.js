/**
 * Luminous Display - Electron Main Process
 *
 * Features:
 * - Kiosk mode (fullscreen, locked)
 * - Watchdog for crash recovery
 * - Auto-start on boot
 * - Device token management via IPC
 */

const { app, BrowserWindow, ipcMain, screen, globalShortcut, powerSaveBlocker } = require('electron');
const path = require('path');
const fs = require('fs');

// Configuration
const CONFIG = {
  // Kiosk mode settings
  kiosk: {
    enabled: process.env.LUMINOUS_KIOSK !== 'false',
    allowDevTools: process.env.LUMINOUS_DEV === 'true',
    exitKeyCombo: 'CommandOrControl+Shift+Q', // Emergency exit for admins
    exitPinRequired: true,
    exitPin: process.env.LUMINOUS_EXIT_PIN || '1234', // Configurable admin PIN
  },
  // Watchdog settings
  watchdog: {
    enabled: true,
    checkInterval: 5000, // 5 seconds
    maxCrashes: 5,
    crashResetTime: 60000, // 1 minute
  },
  // Display settings
  display: {
    preferredWidth: 1080,
    preferredHeight: 1920, // Portrait mode
    forceOrientation: 'portrait',
  },
  // File paths
  paths: {
    userData: app.getPath('userData'),
    deviceToken: path.join(app.getPath('userData'), 'device-token.json'),
    settings: path.join(app.getPath('userData'), 'settings.json'),
    crashLog: path.join(app.getPath('userData'), 'crash-log.json'),
  },
};

// State
let mainWindow = null;
let watchdogInterval = null;
let powerSaveBlockerId = null;
let crashCount = 0;
let lastCrashTime = 0;

/**
 * Create the main display window
 */
function createWindow() {
  // Get the primary display
  const primaryDisplay = screen.getPrimaryDisplay();
  const { width, height } = primaryDisplay.workAreaSize;

  // Window configuration
  const windowConfig = {
    width: CONFIG.kiosk.enabled ? width : CONFIG.display.preferredWidth,
    height: CONFIG.kiosk.enabled ? height : CONFIG.display.preferredHeight,
    x: 0,
    y: 0,
    fullscreen: CONFIG.kiosk.enabled,
    kiosk: CONFIG.kiosk.enabled,
    frame: !CONFIG.kiosk.enabled,
    resizable: !CONFIG.kiosk.enabled,
    minimizable: !CONFIG.kiosk.enabled,
    maximizable: !CONFIG.kiosk.enabled,
    closable: !CONFIG.kiosk.enabled || CONFIG.kiosk.allowDevTools,
    alwaysOnTop: CONFIG.kiosk.enabled,
    skipTaskbar: CONFIG.kiosk.enabled,
    autoHideMenuBar: true,
    backgroundColor: '#FDFCFA', // Canvas color from design system
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      preload: path.join(__dirname, 'preload.js'),
      devTools: CONFIG.kiosk.allowDevTools,
      // Performance optimizations for always-on display
      backgroundThrottling: false,
      // Security
      sandbox: true,
    },
    icon: path.join(__dirname, '../assets/icons/icon.png'),
  };

  mainWindow = new BrowserWindow(windowConfig);

  // Load the Angular app
  const isDev = process.env.LUMINOUS_DEV === 'true';
  if (isDev) {
    mainWindow.loadURL('http://localhost:4200');
  } else {
    mainWindow.loadFile(path.join(__dirname, '../dist/display/browser/index.html'));
  }

  // Prevent navigation outside the app
  mainWindow.webContents.on('will-navigate', (event, url) => {
    const appUrl = isDev ? 'http://localhost:4200' : 'file://';
    if (!url.startsWith(appUrl)) {
      event.preventDefault();
      console.log('Blocked navigation to:', url);
    }
  });

  // Prevent new windows
  mainWindow.webContents.setWindowOpenHandler(() => {
    return { action: 'deny' };
  });

  // Handle crashes
  mainWindow.webContents.on('render-process-gone', (event, details) => {
    console.error('Renderer process gone:', details);
    handleCrash('renderer-crash', details);
  });

  mainWindow.webContents.on('unresponsive', () => {
    console.error('Window unresponsive');
    handleCrash('unresponsive');
  });

  mainWindow.on('responsive', () => {
    console.log('Window responsive again');
  });

  // Handle window close
  mainWindow.on('close', (event) => {
    if (CONFIG.kiosk.enabled && !app.isQuitting) {
      event.preventDefault();
      console.log('Close prevented in kiosk mode');
    }
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });

  // Open DevTools in dev mode
  if (CONFIG.kiosk.allowDevTools) {
    mainWindow.webContents.openDevTools({ mode: 'detach' });
  }

  console.log(`Display window created (kiosk: ${CONFIG.kiosk.enabled})`);
}

/**
 * Handle application crash/freeze
 */
function handleCrash(reason, details = {}) {
  const now = Date.now();

  // Reset crash count if enough time has passed
  if (now - lastCrashTime > CONFIG.watchdog.crashResetTime) {
    crashCount = 0;
  }

  crashCount++;
  lastCrashTime = now;

  // Log crash
  const crashEntry = {
    timestamp: new Date().toISOString(),
    reason,
    details,
    crashCount,
  };
  logCrash(crashEntry);

  if (crashCount >= CONFIG.watchdog.maxCrashes) {
    console.error('Too many crashes, exiting...');
    app.isQuitting = true;
    app.quit();
    return;
  }

  // Attempt recovery
  console.log(`Attempting recovery (crash ${crashCount}/${CONFIG.watchdog.maxCrashes})`);
  setTimeout(() => {
    if (mainWindow) {
      mainWindow.destroy();
    }
    createWindow();
  }, 1000);
}

/**
 * Log crash to file for diagnostics
 */
function logCrash(entry) {
  try {
    let crashes = [];
    if (fs.existsSync(CONFIG.paths.crashLog)) {
      crashes = JSON.parse(fs.readFileSync(CONFIG.paths.crashLog, 'utf8'));
    }
    crashes.push(entry);
    // Keep only last 100 crashes
    if (crashes.length > 100) {
      crashes = crashes.slice(-100);
    }
    fs.writeFileSync(CONFIG.paths.crashLog, JSON.stringify(crashes, null, 2));
  } catch (error) {
    console.error('Failed to log crash:', error);
  }
}

/**
 * Setup watchdog to monitor window health
 */
function setupWatchdog() {
  if (!CONFIG.watchdog.enabled) return;

  watchdogInterval = setInterval(() => {
    if (!mainWindow || mainWindow.isDestroyed()) {
      console.log('Watchdog: Window missing, recreating...');
      createWindow();
    }
  }, CONFIG.watchdog.checkInterval);

  console.log('Watchdog started');
}

/**
 * Setup keyboard shortcuts
 */
function setupShortcuts() {
  // Emergency exit (for admins)
  if (CONFIG.kiosk.exitKeyCombo) {
    globalShortcut.register(CONFIG.kiosk.exitKeyCombo, () => {
      if (CONFIG.kiosk.exitPinRequired) {
        // Send IPC to show PIN dialog
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send('show-exit-dialog');
        }
      } else {
        console.log('Admin exit triggered');
        app.isQuitting = true;
        app.quit();
      }
    });
  }

  // Refresh shortcut - reload the app properly
  const refreshRegistered = globalShortcut.register('CommandOrControl+R', () => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      // Reload by loading the original URL/file to ensure proper initialization
      const isDev = process.env.LUMINOUS_DEV === 'true';
      if (isDev) {
        mainWindow.loadURL('http://localhost:4200');
      } else {
        mainWindow.loadFile(path.join(__dirname, '../dist/display/browser/index.html'));
      }
    }
  });
  if (!refreshRegistered) {
    console.warn('Failed to register Ctrl+R shortcut');
  }

  // Toggle fullscreen (dev mode only)
  if (CONFIG.kiosk.allowDevTools) {
    globalShortcut.register('F11', () => {
      if (mainWindow && !mainWindow.isDestroyed()) {
        mainWindow.setFullScreen(!mainWindow.isFullScreen());
      }
    });
  }
}

/**
 * Setup IPC handlers for communication with renderer
 */
function setupIPC() {
  // Device token management
  ipcMain.handle('get-device-token', async () => {
    try {
      if (fs.existsSync(CONFIG.paths.deviceToken)) {
        const data = fs.readFileSync(CONFIG.paths.deviceToken, 'utf8');
        return JSON.parse(data);
      }
      return null;
    } catch (error) {
      console.error('Failed to read device token:', error);
      return null;
    }
  });

  ipcMain.handle('set-device-token', async (event, tokenData) => {
    try {
      fs.writeFileSync(CONFIG.paths.deviceToken, JSON.stringify(tokenData, null, 2));
      return true;
    } catch (error) {
      console.error('Failed to save device token:', error);
      return false;
    }
  });

  ipcMain.handle('clear-device-token', async () => {
    try {
      if (fs.existsSync(CONFIG.paths.deviceToken)) {
        fs.unlinkSync(CONFIG.paths.deviceToken);
      }
      return true;
    } catch (error) {
      console.error('Failed to clear device token:', error);
      return false;
    }
  });

  // Settings management
  ipcMain.handle('get-settings', async () => {
    try {
      if (fs.existsSync(CONFIG.paths.settings)) {
        const data = fs.readFileSync(CONFIG.paths.settings, 'utf8');
        return JSON.parse(data);
      }
      return {};
    } catch (error) {
      console.error('Failed to read settings:', error);
      return {};
    }
  });

  ipcMain.handle('set-settings', async (event, settings) => {
    try {
      fs.writeFileSync(CONFIG.paths.settings, JSON.stringify(settings, null, 2));
      return true;
    } catch (error) {
      console.error('Failed to save settings:', error);
      return false;
    }
  });

  // App info
  ipcMain.handle('get-app-info', async () => {
    return {
      version: app.getVersion(),
      name: app.getName(),
      isKiosk: CONFIG.kiosk.enabled,
      isDev: process.env.LUMINOUS_DEV === 'true',
    };
  });

  // Admin exit with PIN verification
  ipcMain.handle('verify-exit-pin', async (event, pin) => {
    if (pin === CONFIG.kiosk.exitPin) {
      console.log('Admin exit verified');
      app.isQuitting = true;
      // Destroy the window first to ensure clean exit from kiosk mode
      if (mainWindow && !mainWindow.isDestroyed()) {
        mainWindow.destroy();
      }
      setTimeout(() => app.quit(), 100);
      return true;
    }
    return false;
  });

  // Reload window
  ipcMain.handle('reload-window', async () => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      mainWindow.reload();
    }
  });

  // Get display info
  ipcMain.handle('get-display-info', async () => {
    const primaryDisplay = screen.getPrimaryDisplay();
    return {
      width: primaryDisplay.size.width,
      height: primaryDisplay.size.height,
      scaleFactor: primaryDisplay.scaleFactor,
      rotation: primaryDisplay.rotation,
      bounds: primaryDisplay.bounds,
    };
  });
}

/**
 * Prevent system sleep (for always-on display)
 */
function preventSleep() {
  if (powerSaveBlockerId === null) {
    powerSaveBlockerId = powerSaveBlocker.start('prevent-display-sleep');
    console.log('Power save blocker started:', powerSaveBlockerId);
  }
}

/**
 * Allow system sleep
 */
function allowSleep() {
  if (powerSaveBlockerId !== null && powerSaveBlocker.isStarted(powerSaveBlockerId)) {
    powerSaveBlocker.stop(powerSaveBlockerId);
    powerSaveBlockerId = null;
    console.log('Power save blocker stopped');
  }
}

// App lifecycle
app.whenReady().then(() => {
  console.log('Luminous Display starting...');
  console.log('User data path:', CONFIG.paths.userData);

  // Ensure user data directory exists
  if (!fs.existsSync(CONFIG.paths.userData)) {
    fs.mkdirSync(CONFIG.paths.userData, { recursive: true });
  }

  createWindow();
  setupWatchdog();
  setupShortcuts();
  setupIPC();

  // Prevent sleep in kiosk mode
  if (CONFIG.kiosk.enabled) {
    preventSleep();
  }

  // Handle activate (macOS)
  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

app.on('window-all-closed', () => {
  // Recreate window in kiosk mode unless quitting
  if (CONFIG.kiosk.enabled && !app.isQuitting) {
    console.log('All windows closed in kiosk mode, recreating...');
    createWindow();
    return;
  }

  // In kiosk mode, always quit regardless of platform
  if (CONFIG.kiosk.enabled || process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('will-quit', () => {
  // Cleanup
  if (watchdogInterval) {
    clearInterval(watchdogInterval);
  }
  globalShortcut.unregisterAll();
  allowSleep();
  console.log('Luminous Display shutting down');
});

// Handle uncaught exceptions
process.on('uncaughtException', (error) => {
  console.error('Uncaught exception:', error);
  handleCrash('uncaught-exception', { message: error.message, stack: error.stack });
});

process.on('unhandledRejection', (reason) => {
  console.error('Unhandled rejection:', reason);
});

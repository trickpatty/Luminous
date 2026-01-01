/**
 * Luminous Display - Electron Preload Script
 *
 * Exposes secure IPC channels to the renderer process.
 * Uses context isolation for security.
 */

const { contextBridge, ipcRenderer } = require('electron');

// Expose protected methods to the renderer process
contextBridge.exposeInMainWorld('electronAPI', {
  // Device token management
  getDeviceToken: () => ipcRenderer.invoke('get-device-token'),
  setDeviceToken: (tokenData) => ipcRenderer.invoke('set-device-token', tokenData),
  clearDeviceToken: () => ipcRenderer.invoke('clear-device-token'),

  // Settings management
  getSettings: () => ipcRenderer.invoke('get-settings'),
  setSettings: (settings) => ipcRenderer.invoke('set-settings', settings),

  // App info
  getAppInfo: () => ipcRenderer.invoke('get-app-info'),
  getDisplayInfo: () => ipcRenderer.invoke('get-display-info'),

  // Window control
  reloadWindow: () => ipcRenderer.invoke('reload-window'),

  // Admin functions
  verifyExitPin: (pin) => ipcRenderer.invoke('verify-exit-pin', pin),

  // Event listeners
  onShowExitDialog: (callback) => {
    ipcRenderer.on('show-exit-dialog', () => callback());
    return () => ipcRenderer.removeAllListeners('show-exit-dialog');
  },

  // Platform info
  platform: process.platform,
  isElectron: true,
});

// Log preload completion
console.log('Luminous Display preload script loaded');

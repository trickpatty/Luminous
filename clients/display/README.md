# Luminous Display Application

The Luminous Display Application is an Angular + Electron app designed for wall-mounted family command center displays. It provides a glanceable view of household schedules, tasks, and routines optimized for portrait-mode touchscreens.

## Features

- **Kiosk Mode**: Fullscreen, locked display with admin exit PIN
- **Device Linking**: 6-digit code pairing with family accounts
- **Offline Support**: IndexedDB caching for uninterrupted operation
- **Time-Adaptive Canvas**: Background color shifts throughout the day
- **Auto-Start**: Configurable system boot integration
- **Crash Recovery**: Watchdog process for automatic recovery

## Requirements

- Node.js 18+
- npm 9+
- Electron 33+

## Development

### Install Dependencies

```bash
npm install
```

### Run in Development Mode

```bash
# Start Angular development server
npm start

# Run Electron in dev mode (in another terminal)
npm run electron:dev
```

### Build for Production

```bash
# Build Angular and package with Electron
npm run electron:build

# Platform-specific builds
npm run electron:build:linux
npm run electron:build:win
npm run electron:build:mac
```

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `LUMINOUS_KIOSK` | Enable kiosk mode | `true` |
| `LUMINOUS_DEV` | Enable development features | `false` |
| `LUMINOUS_EXIT_PIN` | Admin exit PIN | `1234` |

## Deployment

### Linux (Raspberry Pi / Kiosk)

1. Build the application:
   ```bash
   npm run electron:build:linux
   ```

2. Copy to target device:
   ```bash
   scp release/luminous-display*.AppImage pi@display:/opt/luminous-display/
   ```

3. Run the kiosk setup script:
   ```bash
   sudo ./scripts/kiosk-setup.sh
   ```

4. Install auto-start:
   ```bash
   sudo ./scripts/install-autostart.sh
   ```

5. Reboot:
   ```bash
   sudo reboot
   ```

### Windows

1. Build the application:
   ```bash
   npm run electron:build:win
   ```

2. Run the installer from `release/` directory

3. Configure auto-start (run as Administrator):
   ```powershell
   .\scripts\install-autostart.ps1
   ```

## Architecture

```
clients/display/
├── electron/          # Electron main process
│   ├── main.js        # Main process with kiosk mode
│   └── preload.js     # Secure IPC bridge
├── src/
│   ├── app/
│   │   ├── core/      # Services, guards, interceptors
│   │   ├── features/  # Display, linking, settings views
│   │   └── shared/    # Shared components
│   ├── styles/        # Design system tokens
│   └── environments/  # Environment configs
└── scripts/           # Deployment scripts
```

## Device Linking Flow

1. Display shows a 6-digit code
2. User enters code in Luminous web/mobile app
3. Server validates and links device to family
4. Display receives device token
5. Token stored securely in Electron user data

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+Q` | Show admin exit dialog |
| `Ctrl+R` | Reload display |
| `F11` | Toggle fullscreen (dev mode only) |

## Contributing

See [CLAUDE.md](../../CLAUDE.md) for development guidelines.

## License

AGPL-3.0

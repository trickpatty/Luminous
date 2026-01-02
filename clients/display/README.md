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

# macOS architecture-specific builds
npm run electron:build:mac-intel    # Intel x64
npm run electron:build:mac-arm      # Apple Silicon arm64
npm run electron:build:mac-universal # Universal binary

# Build all platforms
npm run electron:build:all
```

## CI/CD

The display app uses GitHub Actions for automated builds and releases.

### Automatic Builds

Builds are triggered on:
- Push to `main` or `develop` branches (when `clients/display/` changes)
- Pull requests to `main` or `develop`
- Git tags matching `display-v*`
- Manual workflow dispatch

### Creating a Release

1. **Via Git Tag** (recommended):
   ```bash
   git tag display-v0.1.0
   git push origin display-v0.1.0
   ```

2. **Via Manual Dispatch**:
   - Go to Actions → "Electron Display Build and Release"
   - Click "Run workflow"
   - Check "Create a GitHub release"
   - Enter version number (e.g., `0.1.0`)

### Build Artifacts

| Platform | Architecture | Artifact |
|----------|--------------|----------|
| Windows | x64 | `.exe` installer, `.exe` portable |
| macOS | Intel (x64) | `.dmg`, `.zip` |
| macOS | Apple Silicon (arm64) | `.dmg`, `.zip` |
| Linux | x64, arm64 | `.AppImage`, `.deb`, `.tar.gz` |

### Code Signing (Optional)

For signed macOS builds, configure these repository secrets:
- `MAC_CERTS`: Base64-encoded p12 certificate
- `MAC_CERTS_PASSWORD`: Certificate password
- `APPLE_ID`: Apple Developer ID email
- `APPLE_APP_SPECIFIC_PASSWORD`: App-specific password
- `APPLE_TEAM_ID`: Apple Developer Team ID

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

The display uses a "device shows code, user enters code" pattern for secure linking:

1. **On the Display**: Tap "Get Link Code" to generate a unique 6-digit code
2. **On the Display**: The code appears on screen (valid for 15 minutes)
3. **On Web/Mobile App**: An admin logs in and goes to Devices
4. **On Web/Mobile App**: Click "Link New Device", enter the code shown on the display
5. **Server**: Validates the code and links the device to the family
6. **On the Display**: Automatically receives authentication token and syncs family data
7. **Token Storage**: Credentials stored securely in Electron user data

> **Note**: The display generates the code and waits; the authenticated user in the web/mobile app enters the code to complete linking.

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

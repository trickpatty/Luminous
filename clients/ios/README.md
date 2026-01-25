# Luminous iOS App

The Luminous iOS app provides on-the-go access to your family command center with push notifications, quick actions, and full feature parity with the web application.

## Requirements

- **Xcode 15.0+** (for iOS 17 SDK)
- **iOS 17.0+** deployment target
- **Swift 5.9+**
- **macOS Sonoma 14.0+** (for development)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/luminous-app/luminous.git
cd luminous/clients/ios
```

### 2. Create Xcode Project

Since the Xcode project files cannot be checked into version control as source files, you'll need to create the project:

1. Open Xcode
2. Create a new project: **File > New > Project**
3. Select **iOS > App**
4. Configure the project:
   - **Product Name:** `Luminous`
   - **Organization Identifier:** `com.luminous`
   - **Interface:** SwiftUI
   - **Language:** Swift
   - **Include Tests:** Yes
5. Save in the `clients/ios` directory
6. Drag the existing `Luminous` folder into the project navigator
7. When prompted, select **"Copy items if needed"** and add to the `Luminous` target

### 3. Add Swift Package Dependencies

Go to **File > Add Package Dependencies** and add each package:

| Package | URL | Version |
|---------|-----|---------|
| Alamofire | `https://github.com/Alamofire/Alamofire.git` | 5.8.0+ |
| KeychainSwift | `https://github.com/evgenyneu/keychain-swift.git` | 21.0.0+ |
| SignalR-Client-Swift | `https://github.com/moozzyk/SignalR-Client-Swift.git` | 0.9.0+ |
| Nuke | `https://github.com/kean/Nuke.git` | 12.0.0+ |
| swift-openapi-runtime | `https://github.com/apple/swift-openapi-runtime.git` | 1.0.0+ |
| swift-openapi-urlsession | `https://github.com/apple/swift-openapi-urlsession.git` | 1.0.0+ |
| SwiftyJSON | `https://github.com/SwiftyJSON/SwiftyJSON.git` | 5.0.0+ |

### 4. Configure Signing

1. Select the project in the navigator
2. Select the **Luminous** target
3. Go to **Signing & Capabilities**
4. Select your development team
5. Enable **"Automatically manage signing"**

### 5. Add Capabilities

Add the following capabilities under **Signing & Capabilities**:

- **Associated Domains** (for passkeys and Universal Links)
  - Add: `webcredentials:luminous.app`
  - Add: `applinks:luminous.app`
- **Push Notifications**
- **Background Modes**
  - Background fetch
  - Remote notifications
- **Sign in with Apple** (optional)

### 6. Configure Info.plist

Add the following keys to your Info.plist:

```xml
<!-- Face ID / Biometric Authentication -->
<key>NSFaceIDUsageDescription</key>
<string>Luminous uses Face ID for quick and secure sign-in</string>

<!-- Camera for QR Code Scanning -->
<key>NSCameraUsageDescription</key>
<string>Luminous uses your camera to scan QR codes for device linking</string>

<!-- Deep Linking -->
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>luminous</string>
        </array>
        <key>CFBundleURLName</key>
        <string>com.luminous.app</string>
    </dict>
</array>
```

### 7. Build and Run

1. Select your target device or simulator
2. Press **Cmd + R** or click the **Run** button

## Project Structure

```
Luminous/
├── App/
│   ├── AppState.swift              # Global app state
│   └── AppConfiguration.swift      # Environment configuration
├── Core/
│   └── Services/
│       ├── APIService.swift        # HTTP client with retry logic
│       ├── AuthService.swift       # Authentication (passkey, OTP)
│       └── KeychainService.swift   # Secure token storage
├── Design/
│   ├── DesignTokens.swift          # Colors, spacing, typography
│   └── Components/
│       ├── LuminousButton.swift
│       ├── LuminousCard.swift
│       ├── LuminousAvatar.swift
│       ├── LuminousTextField.swift
│       └── EmptyStateView.swift
├── Features/
│   ├── Auth/
│   │   ├── AuthenticationView.swift
│   │   └── AuthViewModel.swift
│   ├── Home/
│   │   ├── HomeView.swift
│   │   └── HomeViewModel.swift
│   ├── Calendar/
│   │   ├── CalendarView.swift
│   │   └── CalendarViewModel.swift
│   ├── Tasks/
│   │   ├── TasksView.swift
│   │   └── TasksViewModel.swift
│   ├── Family/
│   │   ├── FamilyView.swift
│   │   └── FamilyViewModel.swift
│   ├── Settings/
│   │   └── SettingsView.swift
│   └── Main/
│       └── MainTabView.swift
├── Generated/                      # OpenAPI-generated API client
├── Resources/
│   ├── Assets.xcassets/
│   └── Localizable.strings
├── LuminousApp.swift               # App entry point
└── ContentView.swift               # Root view
```

## Architecture

The app follows **MVVM (Model-View-ViewModel)** architecture:

### Views (`*View.swift`)
- SwiftUI views that define the UI
- Subscribe to ViewModels using `@StateObject` or `@EnvironmentObject`
- Minimal logic; delegate to ViewModels

### ViewModels (`*ViewModel.swift`)
- `@MainActor` classes with `@Published` properties
- Handle business logic and state management
- Interact with services via protocols for testability

### Services (`*Service.swift`)
- Stateless (mostly) singletons for specific concerns
- `APIService`: HTTP requests with retry and auth headers
- `AuthService`: Passkey and OTP authentication
- `KeychainService`: Secure storage (actor-based for thread safety)

### Models
- Data models matching API contracts
- Decodable/Encodable for JSON serialization
- Domain models for UI-specific needs

## Design System

The app uses design tokens from the shared `design-tokens/` directory:

- **Colors**: Time-adaptive canvas, accent, status, member colors
- **Spacing**: Consistent spacing scale (4px base)
- **Typography**: Font sizes for display, title, body, caption
- **Touch targets**: Minimum 44pt for accessibility
- **Border radius**: Small to full (pills/circles)

Design tokens are exported to `Design/DesignTokens.swift` using Style Dictionary.

## API Client

The API client is auto-generated from the OpenAPI specification:

1. The `openapi-clients.yml` workflow generates Swift client code
2. Generated files are placed in `Generated/`
3. The client uses `swift-openapi-runtime` for type-safe API calls

To regenerate manually:

```bash
# From the repository root
swift-openapi-generator generate openapi/v1.json \
  --config clients/ios/openapi-generator-config.yaml \
  --output-directory clients/ios/Generated
```

## Authentication

The app supports two authentication methods:

### Passkeys (Primary)
- Uses `ASAuthorizationController` for WebAuthn
- Provides passwordless, phishing-resistant authentication
- Stored in iCloud Keychain for cross-device sync

### Email OTP (Fallback)
- 6-digit code sent to email
- For devices that don't support passkeys
- Rate-limited for security

## Testing

### Unit Tests
```bash
# Run unit tests
xcodebuild test -scheme Luminous -destination 'platform=iOS Simulator,name=iPhone 15'
```

### UI Tests
```bash
# Run UI tests
xcodebuild test -scheme LuminousUITests -destination 'platform=iOS Simulator,name=iPhone 15'
```

## Build Configurations

| Configuration | Environment | API URL |
|---------------|-------------|---------|
| Debug | debug | `http://localhost:5000` |
| Staging | staging | `https://api-staging.luminous.app` |
| Release | production | `https://api.luminous.app` |

## Deployment

### TestFlight (Beta)
1. Archive the app: **Product > Archive**
2. Upload to App Store Connect
3. Distribute via TestFlight

### App Store
1. Complete App Store Connect listing
2. Submit for review
3. Release to App Store

## Troubleshooting

### Build Errors

**"No such module 'Alamofire'"**
- Ensure Swift Package dependencies are resolved
- **File > Packages > Resolve Package Versions**

**"Signing requires a development team"**
- Select your team in **Signing & Capabilities**

### Runtime Errors

**"Network connection lost"**
- Check API base URL in `AppConfiguration.swift`
- Ensure local backend is running for debug builds

**"Passkey authentication failed"**
- Verify Associated Domains capability
- Check `apple-app-site-association` file on server

## Contributing

1. Follow the [CLAUDE.md](../../CLAUDE.md) guidelines
2. Use conventional commits with gitmoji
3. Write tests for new features
4. Update documentation as needed

## License

AGPL-3.0 - See [LICENSE](../../LICENSE) for details.

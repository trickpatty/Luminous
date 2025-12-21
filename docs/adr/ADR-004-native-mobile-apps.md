# ADR-004: Native iOS and Android Apps

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Technology, Architecture

## Context

Luminous needs companion mobile apps for iOS and Android that provide:
- Full access to family calendar, chores, and lists
- Push notifications for reminders and updates
- Device linking (entering codes to link displays)
- Offline capability with local caching
- Platform-native user experience

We need to decide between cross-platform frameworks and native development.

## Decision Drivers

- **User experience**: Best possible UX on each platform
- **Push notifications**: Reliable APNs and FCM integration
- **Offline support**: Robust local data persistence
- **Performance**: Smooth animations and fast startup
- **Platform features**: Widgets, biometrics, deep linking
- **App Store approval**: Reliable acceptance on both stores

## Considered Options

### Option 1: Native (Swift for iOS, Kotlin for Android)

Separate native applications for each platform.

**Pros:**
- Best possible UX and performance
- Full access to platform APIs
- Most reliable push notifications
- Platform-native patterns and conventions
- Best App Store acceptance rates
- Mature tooling (Xcode, Android Studio)

**Cons:**
- Two codebases to maintain
- Different languages (Swift, Kotlin)
- Higher development cost
- Separate teams/skills needed

### Option 2: React Native

Cross-platform JavaScript/TypeScript framework.

**Pros:**
- Single codebase (mostly)
- JavaScript/TypeScript language
- Could share code with React web (not applicable here)

**Cons:**
- Not using React for web (Angular)
- Bridge overhead affects performance
- Native module updates can lag
- Push notification setup still complex

### Option 3: Flutter

Cross-platform Dart framework.

**Pros:**
- Single codebase
- Great performance
- Beautiful UI toolkit

**Cons:**
- Dart language (not TypeScript)
- Separate ecosystem from web
- Large app size
- Less mature than native

### Option 4: Kotlin Multiplatform

Shared Kotlin code with native UI.

**Pros:**
- Shared business logic
- Native UI on both platforms
- Better than full cross-platform

**Cons:**
- Relatively new technology
- iOS support still maturing
- Smaller community

## Decision

We will build **native mobile apps**: Swift/SwiftUI for iOS and Kotlin/Jetpack Compose for Android.

## Rationale

1. **User Experience**: Native apps provide the best possible experience on each platform, following platform conventions and guidelines.

2. **Push Notifications**: Direct APNs and FCM integration is more reliable than cross-platform abstraction layers.

3. **Offline/Caching**: Native persistence (Core Data, Room) is more robust and performant than cross-platform alternatives.

4. **Platform Features**: Widgets, biometrics, and other platform features work best with native code.

5. **App Store Success**: Native apps have highest acceptance rates and best store presence.

6. **No Code Sharing Need**: Since web uses Angular (not React), there's no code sharing benefit from React Native. We're building separate clients anyway.

## Consequences

### Positive

- Best possible user experience on each platform
- Most reliable push notifications
- Full access to platform features
- Highest App Store acceptance rates
- Platform-native offline support

### Negative

- Two separate codebases
- Need Swift and Kotlin expertise
- Higher development effort
- API contract must be well-defined

### Neutral

- Can hire platform specialists
- Need shared API client design
- Must maintain feature parity between platforms

## Implementation Notes

### iOS (Swift/SwiftUI)
- SwiftUI for modern declarative UI
- Core Data for offline persistence
- MSAL for Azure AD B2C authentication
- APNs for push notifications
- WidgetKit for home screen widgets

### Android (Kotlin/Compose)
- Jetpack Compose for modern UI
- Room for offline persistence
- MSAL for Azure AD B2C authentication
- FCM for push notifications
- Glance for app widgets

### Shared
- OpenAPI-generated API clients
- Common API contract
- Feature parity checklist

## Related Decisions

- [ADR-001: .NET 10 as Backend Platform](./ADR-001-dotnet-backend.md)
- [ADR-010: Azure AD B2C for Identity](./ADR-010-azure-ad-b2c-identity.md)

## References

- [SwiftUI Documentation](https://developer.apple.com/xcode/swiftui/)
- [Jetpack Compose Documentation](https://developer.android.com/jetpack/compose)
- [MSAL for iOS and Android](https://learn.microsoft.com/azure/active-directory/develop/msal-overview)

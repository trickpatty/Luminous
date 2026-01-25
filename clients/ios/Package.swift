// swift-tools-version:5.9
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

let package = Package(
    name: "Luminous",
    platforms: [
        .iOS(.v17),
        .macOS(.v14)
    ],
    products: [
        .library(
            name: "LuminousCore",
            targets: ["LuminousCore"]
        ),
        .library(
            name: "LuminousDesign",
            targets: ["LuminousDesign"]
        ),
    ],
    dependencies: [
        // Networking
        .package(url: "https://github.com/Alamofire/Alamofire.git", from: "5.8.0"),

        // Keychain
        .package(url: "https://github.com/evgenyneu/keychain-swift.git", from: "21.0.0"),

        // SignalR for real-time sync
        .package(url: "https://github.com/moozzyk/SignalR-Client-Swift.git", from: "0.9.0"),

        // Image caching and loading
        .package(url: "https://github.com/kean/Nuke.git", from: "12.0.0"),

        // OpenAPI runtime (for generated API client)
        .package(url: "https://github.com/apple/swift-openapi-runtime.git", from: "1.0.0"),
        .package(url: "https://github.com/apple/swift-openapi-urlsession.git", from: "1.0.0"),

        // JSON handling
        .package(url: "https://github.com/SwiftyJSON/SwiftyJSON.git", from: "5.0.0"),

        // Core Data utilities (for offline caching)
        .package(url: "https://github.com/JohnSundell/Codextended.git", from: "0.3.0"),
    ],
    targets: [
        // Core business logic and services
        .target(
            name: "LuminousCore",
            dependencies: [
                "Alamofire",
                .product(name: "KeychainSwift", package: "keychain-swift"),
                .product(name: "SignalRClient", package: "SignalR-Client-Swift"),
                "SwiftyJSON",
                .product(name: "OpenAPIRuntime", package: "swift-openapi-runtime"),
                .product(name: "OpenAPIURLSession", package: "swift-openapi-urlsession"),
            ],
            path: "Luminous/Core"
        ),

        // Design system and UI components
        .target(
            name: "LuminousDesign",
            dependencies: [
                "Nuke",
                .product(name: "NukeUI", package: "Nuke"),
            ],
            path: "Luminous/Design"
        ),

        // Test targets
        .testTarget(
            name: "LuminousCoreTests",
            dependencies: ["LuminousCore"],
            path: "LuminousTests/Core"
        ),
        .testTarget(
            name: "LuminousDesignTests",
            dependencies: ["LuminousDesign"],
            path: "LuminousTests/Design"
        ),
    ]
)

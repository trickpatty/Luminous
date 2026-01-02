# Luminous Icons

Central location for application icons used across all platforms.

## Files

| File | Description |
|------|-------------|
| `icon.png` | Source icon (512x512 or 1024x1024 PNG) |

## Usage

The CI/CD pipeline automatically converts `icon.png` to platform-specific formats:

- **Linux**: Multiple PNG sizes (16-512px)
- **Windows**: `.ico` file with multiple sizes
- **macOS**: `.icns` file with @2x Retina variants

## Requirements

- **Format**: PNG with transparency support
- **Size**: 512x512 pixels minimum (1024x1024 recommended)
- **Color Space**: sRGB

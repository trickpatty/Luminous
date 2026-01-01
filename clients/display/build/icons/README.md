# Luminous Display Icons

This folder should contain the application icons for each platform.

## Required Files

| File | Platform | Size/Format |
|------|----------|-------------|
| `icon.png` | Linux | 512x512 PNG |
| `icon.ico` | Windows | Multi-size ICO (16, 32, 48, 64, 128, 256) |
| `icon.icns` | macOS | Multi-size ICNS (16, 32, 64, 128, 256, 512, 1024) |

## Icon Generation

You can generate all required formats from a single 1024x1024 PNG using tools like:

### Using electron-icon-builder (recommended)

```bash
npm install -g electron-icon-builder
electron-icon-builder --input=source-icon-1024.png --output=./build
```

### Using online tools

- [CloudConvert](https://cloudconvert.com/) - Convert PNG to ICO/ICNS
- [iConvert Icons](https://iconverticons.com/) - Generate all formats

### Manual creation

1. **Linux (icon.png)**: Export a 512x512 PNG with transparency
2. **Windows (icon.ico)**: Use [GIMP](https://www.gimp.org/) or [IcoFX](https://icofx.ro/) to create multi-size ICO
3. **macOS (icon.icns)**: Use `iconutil` on macOS:
   ```bash
   mkdir icon.iconset
   sips -z 16 16 icon-1024.png --out icon.iconset/icon_16x16.png
   sips -z 32 32 icon-1024.png --out icon.iconset/icon_16x16@2x.png
   # ... repeat for all sizes
   iconutil -c icns icon.iconset
   ```

## Placeholder

Until production icons are created, the build will use electron-builder's default icons.
To create a quick placeholder, you can use a 512x512 PNG with a simple "L" logo.

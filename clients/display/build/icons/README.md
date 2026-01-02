# Luminous Display Icons

This folder contains the application icons for the Electron display application.

## Adding Your Custom Icon

To use a custom icon for all platforms, simply place your icon as `icon.png` in this folder:

```
clients/display/build/icons/icon.png
```

### Requirements

- **Format**: PNG with transparency support
- **Size**: 512x512 pixels minimum (1024x1024 recommended for best quality)
- **Color Space**: sRGB

The CI/CD pipeline will automatically:
- Generate all required sizes for Linux (16, 32, 48, 64, 128, 256, 512px)
- Generate Windows `.ico` file with multiple sizes
- Generate macOS `.icns` file with all required sizes including @2x variants

### Placeholder Behavior

If no `icon.png` is present in this folder, the CI/CD pipeline will generate a simple placeholder icon (blue gradient with "L" text) for builds to succeed.

## Manual Icon Generation

If you need to generate icons locally, you can use the following tools:

### Using electron-icon-builder (recommended)

```bash
npm install -g electron-icon-builder
electron-icon-builder --input=icon.png --output=./build
```

### Using ImageMagick

```bash
# Linux (multiple sizes)
for size in 16 32 48 64 128 256 512; do
  convert icon.png -resize ${size}x${size} ${size}x${size}.png
done

# Windows (.ico)
convert icon.png -define icon:auto-resize=256,128,64,48,32,16 ../icon.ico

# macOS (.icns) - requires macOS
mkdir icon.iconset
for size in 16 32 64 128 256 512; do
  sips -z $size $size icon.png --out icon.iconset/icon_${size}x${size}.png
  sips -z $((size*2)) $((size*2)) icon.png --out icon.iconset/icon_${size}x${size}@2x.png
done
iconutil -c icns icon.iconset -o ../icon.icns
```

## Icon Files Reference

| File | Platform | Description |
|------|----------|-------------|
| `icon.png` | Source | Base icon (512x512 or 1024x1024 PNG) |
| `../icon.ico` | Windows | Multi-size ICO (16-256px) |
| `../icon.icns` | macOS | Multi-size ICNS with @2x variants |
| `{size}x{size}.png` | Linux | Individual size PNG files |

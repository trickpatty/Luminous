/**
 * Color utility functions for Luminous applications.
 */

import { MEMBER_COLORS, MemberColorName } from '../components/avatar/avatar.component';

/**
 * Get a member color by name
 */
export function getMemberColor(colorName: MemberColorName): string {
  return MEMBER_COLORS[colorName];
}

/**
 * Get a member color with opacity
 * @param colorName Member color name
 * @param opacity Opacity value (0-1)
 */
export function getMemberColorWithOpacity(colorName: MemberColorName, opacity: number): string {
  const hex = MEMBER_COLORS[colorName];
  return hexToRgba(hex, opacity);
}

/**
 * Convert hex color to rgba
 * @param hex Hex color string (e.g., "#FF5733" or "FF5733")
 * @param alpha Alpha value (0-1)
 */
export function hexToRgba(hex: string, alpha = 1): string {
  const cleanHex = hex.replace('#', '');
  const r = parseInt(cleanHex.substring(0, 2), 16);
  const g = parseInt(cleanHex.substring(2, 4), 16);
  const b = parseInt(cleanHex.substring(4, 6), 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}

/**
 * Convert hex color to RGB object
 */
export function hexToRgb(hex: string): { r: number; g: number; b: number } {
  const cleanHex = hex.replace('#', '');
  return {
    r: parseInt(cleanHex.substring(0, 2), 16),
    g: parseInt(cleanHex.substring(2, 4), 16),
    b: parseInt(cleanHex.substring(4, 6), 16),
  };
}

/**
 * Determine if text on a background color should be light or dark
 * @param backgroundColor Hex color of the background
 * @returns true if text should be dark (black), false if light (white)
 */
export function shouldUseDarkText(backgroundColor: string): boolean {
  const { r, g, b } = hexToRgb(backgroundColor);
  // Using relative luminance formula
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
  return luminance > 0.5;
}

/**
 * Get a random member color
 */
export function getRandomMemberColor(): MemberColorName {
  const colorNames = Object.keys(MEMBER_COLORS) as MemberColorName[];
  return colorNames[Math.floor(Math.random() * colorNames.length)];
}

/**
 * Get a consistent color based on a string (e.g., user name)
 * @param str String to hash for color selection
 */
export function getColorFromString(str: string): MemberColorName {
  const colorNames = Object.keys(MEMBER_COLORS) as MemberColorName[];
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  return colorNames[Math.abs(hash) % colorNames.length];
}

/**
 * All available member colors
 */
export const memberColorNames: MemberColorName[] = Object.keys(MEMBER_COLORS) as MemberColorName[];

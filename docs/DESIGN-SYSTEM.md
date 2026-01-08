# Luminous Design System

> **Version**: 1.1.0
> **Status**: Canonical Specification
> **Last Updated**: January 2026

This document defines the complete design system for Luminous—every color, component, interaction, and principle needed to create a cohesive, beautiful, and functional family command center.

---

## Table of Contents

1. [Design Philosophy](#1-design-philosophy)
2. [Brand Personality](#2-brand-personality)
3. [Color System](#3-color-system)
4. [Typography](#4-typography)
5. [Spacing & Layout](#5-spacing--layout)
6. [Elevation & Depth](#6-elevation--depth)
7. [Iconography](#7-iconography)
8. [UI Components](#8-ui-components)
9. [Patterns & Templates](#9-patterns--templates)
10. [Motion & Animation](#10-motion--animation)
11. [UX Principles & Flows](#11-ux-principles--flows)
12. [Accessibility](#12-accessibility)
13. [Personalization](#13-personalization)
14. [Platform Adaptations](#14-platform-adaptations)
15. [Implementation Reference](#15-implementation-reference)
16. [Multi-Platform Token Export](#16-multi-platform-token-export)

---

## 1. Design Philosophy

### 1.1 Core Belief

**Luminous is a window, not a screen.**

While most digital products compete for attention, Luminous provides clarity. It's the calm center of a busy household—always present, never demanding. Like a well-designed clock or thermostat, it delivers essential information at a glance and recedes into the environment when not needed.

### 1.2 Design Principles

#### Principle 1: Calm Confidence
The interface exudes quiet assurance. It doesn't shout, flash, or compete for attention. Information is presented with clarity and conviction—large enough to read from across the room, organized enough to parse in two seconds.

#### Principle 2: Warm Welcome
Luminous feels like home. Soft, warm tones create comfort. The interface adapts to the rhythm of the day—bright and energizing in the morning, warm and restful in the evening. It belongs on the wall like a piece of thoughtful furniture.

#### Principle 3: Joyful Clarity
Family life is colorful and personal. Each family member has their own vibrant color that makes their items instantly recognizable. These personal colors pop against the calm canvas, creating visual delight without chaos.

#### Principle 4: Effortless for Everyone
A six-year-old checking their chores and a grandparent viewing the schedule should both succeed without instruction. Touch targets are generous, text is readable, and actions are obvious.

#### Principle 5: Respectful Presence
As an always-on display in a living space, Luminous must be a good neighbor. It doesn't strain eyes, waste energy on unnecessary animation, or display content that doesn't serve the household.

### 1.3 The Luminous Promise

> **From across the room in two seconds, any family member can understand what matters right now.**

Every design decision must serve this promise.

---

## 2. Brand Personality

### 2.1 Voice Characteristics

| Trait | Description | Example |
|-------|-------------|---------|
| **Warm** | Friendly without being cute | "Good morning" not "Rise and shine! ☀️" |
| **Clear** | Direct without being cold | "Soccer at 3:30" not "Upcoming: Soccer practice event" |
| **Calm** | Reassuring without being passive | "All caught up" not "NOTHING TO DO!!!" |
| **Helpful** | Anticipatory without being presumptuous | Shows relevant info at relevant times |

### 2.2 Emotional Tone

**Morning**: Fresh, energizing, ready-for-the-day optimism
**Afternoon**: Productive, balanced, steady progress
**Evening**: Warm, relaxed, winding-down comfort
**Night**: Restful, minimal, respectful of sleep

### 2.3 What Luminous Is NOT

- Not playful or gamified (beyond appropriate reward moments)
- Not corporate or sterile
- Not trendy or attention-seeking
- Not cluttered or information-dense
- Not dark or moody by default

---

## 3. Color System

### 3.1 Adaptive Canvas

The background canvas shifts warmth throughout the day, creating a natural rhythm that reduces eye strain and feels appropriate for living spaces.

#### Time-Based Canvas Colors

| Period | Time | Canvas | CSS Variable |
|--------|------|--------|--------------|
| **Dawn** | 5:00–7:00 | `#FFFCF7` | `--canvas-dawn` |
| **Morning** | 7:00–12:00 | `#FEFDFB` | `--canvas-morning` |
| **Afternoon** | 12:00–17:00 | `#FDFCFA` | `--canvas-afternoon` |
| **Evening** | 17:00–21:00 | `#FDF9F3` | `--canvas-evening` |
| **Night** | 21:00–5:00 | `#FAF7F2` | `--canvas-night` |

**Transition**: Canvas color shifts gradually over 30 minutes at each threshold.

#### Canvas Implementation

```css
:root {
  /* Base canvas - adapts via JavaScript */
  --canvas: #FDFCFA;

  /* Static references for each period */
  --canvas-dawn: #FFFCF7;
  --canvas-morning: #FEFDFB;
  --canvas-afternoon: #FDFCFA;
  --canvas-evening: #FDF9F3;
  --canvas-night: #FAF7F2;
}
```

### 3.2 Surface Colors

Surfaces are the cards, panels, and containers that hold content.

| Surface | Color | Usage |
|---------|-------|-------|
| **Surface Primary** | `#FFFFFF` | Cards, dialogs, primary containers |
| **Surface Secondary** | `#FAFAFA` | Subtle backgrounds, nested containers |
| **Surface Elevated** | `#FFFFFF` | Floating elements, popovers |
| **Surface Interactive** | `#F9FAFB` | Hover state for interactive surfaces |
| **Surface Pressed** | `#F3F4F6` | Active/pressed state |

### 3.3 Text Colors

| Role | Color | Contrast Ratio | Usage |
|------|-------|----------------|-------|
| **Text Primary** | `#111827` | 16:1 on white | Headlines, body text, primary content |
| **Text Secondary** | `#6B7280` | 5.7:1 on white | Supporting text, labels, captions |
| **Text Tertiary** | `#9CA3AF` | 3.5:1 on white | Placeholder text, disabled states |
| **Text Inverse** | `#FFFFFF` | Varies | Text on colored backgrounds |
| **Text On Color** | `#FFFFFF` | Varies | Text on family member colors |

### 3.4 Semantic Colors

Colors that convey meaning consistently throughout the interface.

#### Status Colors

| Status | Primary | Light | Dark | Usage |
|--------|---------|-------|------|-------|
| **Success** | `#16A34A` | `#DCFCE7` | `#166534` | Completed, confirmed, positive |
| **Warning** | `#D97706` | `#FEF3C7` | `#B45309` | Attention needed, upcoming |
| **Danger** | `#DC2626` | `#FEE2E2` | `#B91C1C` | Urgent, errors, destructive |
| **Info** | `#0284C7` | `#E0F2FE` | `#075985` | Informational, neutral highlight |

#### Usage Guidelines

- **Success**: Task completed, routine finished, positive confirmation
- **Warning**: Due soon, needs attention, soft alert
- **Danger**: Overdue, conflict, error state, delete confirmation
- **Info**: General information, tips, neutral notifications

### 3.5 Brand Accent Color

The primary brand color for actions, links, and interactive elements.

| Shade | Color | Usage |
|-------|-------|-------|
| **50** | `#F0F9FF` | Subtle backgrounds |
| **100** | `#E0F2FE` | Light hover states |
| **200** | `#BAE6FD` | Borders, dividers |
| **300** | `#7DD3FC` | Disabled states |
| **400** | `#38BDF8` | Secondary actions |
| **500** | `#0EA5E9` | Primary interactive |
| **600** | `#0284C7` | Primary buttons |
| **700** | `#0369A1` | Hover states |
| **800** | `#075985` | Active/pressed |
| **900** | `#0C4A6E` | Text on light backgrounds |

### 3.6 Family Member Colors

Each family member is assigned a personal color that identifies their items throughout the interface.

#### The Palette

| Name | Primary | Light (12% opacity) | Border (30% opacity) | Text On |
|------|---------|---------------------|----------------------|---------|
| **Sky** | `#0EA5E9` | `rgba(14,165,233,0.12)` | `rgba(14,165,233,0.30)` | White |
| **Emerald** | `#10B981` | `rgba(16,185,129,0.12)` | `rgba(16,185,129,0.30)` | White |
| **Amber** | `#F59E0B` | `rgba(245,158,11,0.12)` | `rgba(245,158,11,0.30)` | Black |
| **Orange** | `#F97316` | `rgba(249,115,22,0.12)` | `rgba(249,115,22,0.30)` | White |
| **Rose** | `#F43F5E` | `rgba(244,63,94,0.12)` | `rgba(244,63,94,0.30)` | White |
| **Violet** | `#8B5CF6` | `rgba(139,92,246,0.12)` | `rgba(139,92,246,0.30)` | White |
| **Pink** | `#EC4899` | `rgba(236,72,153,0.12)` | `rgba(236,72,153,0.30)` | White |
| **Teal** | `#14B8A6` | `rgba(20,184,166,0.12)` | `rgba(20,184,166,0.30)` | White |

#### Color Assignment

Colors are assigned during profile creation. The system suggests available colors to ensure household variety, but users may override. Two members may share a color if desired.

#### Usage Rules

1. **Avatars**: Solid color background with white initials
2. **Task Cards**: Light tint background with colored left border
3. **Calendar Events**: Colored dot or stripe indicator
4. **Completion Celebrations**: Full-color animation burst
5. **Never**: Full-color large surfaces (overwhelming)

### 3.7 Dark Mode (Night Display)

For households that prefer a dark interface at night, an optional Night Display mode is available.

| Element | Light Mode | Night Display |
|---------|------------|---------------|
| Canvas | `#FAF7F2` | `#18181B` |
| Surface | `#FFFFFF` | `#27272A` |
| Surface Secondary | `#FAFAFA` | `#1F1F23` |
| Text Primary | `#111827` | `#FAFAFA` |
| Text Secondary | `#6B7280` | `#A1A1AA` |
| Border | `#E5E7EB` | `#3F3F46` |

**Note**: Night Display is optional and user-enabled, not automatic. The default behavior uses warm canvas shifting, not dark mode.

---

## 4. Typography

### 4.1 Font Stack

Luminous uses system fonts exclusively for optimal performance, instant loading, and platform-native feel.

```css
:root {
  --font-family: system-ui, -apple-system, BlinkMacSystemFont,
                 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;

  --font-family-mono: ui-monospace, SFMono-Regular, 'SF Mono',
                      Menlo, Consolas, monospace;
}
```

### 4.2 Type Scale

A purposeful scale designed for wall-mounted readability and mobile usability.

#### Display Sizes (Wall Display)

For the large portrait display, used for primary glanceable content.

| Name | Size | Line Height | Weight | Usage |
|------|------|-------------|--------|-------|
| **Display XL** | 72px / 4.5rem | 1.1 | 600 | Time display |
| **Display LG** | 56px / 3.5rem | 1.15 | 600 | Day/date header |
| **Display MD** | 40px / 2.5rem | 1.2 | 500 | Section headers |
| **Display SM** | 32px / 2rem | 1.25 | 500 | Card titles |

#### Content Sizes (All Platforms)

For readable content at normal viewing distances.

| Name | Size | Line Height | Weight | Usage |
|------|------|-------------|--------|-------|
| **Title LG** | 24px / 1.5rem | 1.3 | 600 | Page titles, modal headers |
| **Title MD** | 20px / 1.25rem | 1.35 | 600 | Card headers |
| **Title SM** | 18px / 1.125rem | 1.4 | 500 | Subsection titles |
| **Body LG** | 18px / 1.125rem | 1.5 | 400 | Large body text |
| **Body MD** | 16px / 1rem | 1.5 | 400 | Default body text |
| **Body SM** | 14px / 0.875rem | 1.5 | 400 | Secondary content |
| **Caption** | 12px / 0.75rem | 1.4 | 500 | Labels, timestamps |
| **Overline** | 11px / 0.6875rem | 1.3 | 600 | Category labels (uppercase) |

### 4.3 Font Weights

| Weight | Value | Usage |
|--------|-------|-------|
| **Regular** | 400 | Body text, descriptions |
| **Medium** | 500 | Emphasis, interactive labels |
| **Semibold** | 600 | Titles, headers, buttons |
| **Bold** | 700 | Strong emphasis (rare) |

### 4.4 Text Styles

#### Glanceable Text

For wall display content that must be readable from 10+ feet.

```css
.text-glanceable {
  font-size: 2rem;        /* 32px */
  font-weight: 500;
  line-height: 1.3;
  letter-spacing: -0.01em;
  -webkit-font-smoothing: antialiased;
}
```

#### Time Display

Large, confident time presentation.

```css
.text-time {
  font-size: 4.5rem;      /* 72px */
  font-weight: 600;
  line-height: 1;
  letter-spacing: -0.02em;
  font-variant-numeric: tabular-nums;
}
```

#### Readable Numbers

For schedules, countdowns, and data.

```css
.text-numeric {
  font-variant-numeric: tabular-nums;
  letter-spacing: 0;
}
```

### 4.5 Typography Rules

1. **Maximum line length**: 65–75 characters for body text
2. **Minimum contrast**: 4.5:1 for body text, 3:1 for large text (WCAG AA)
3. **No ALL CAPS** for sentences (reduces readability)
4. **Sentence case** for UI labels, Title Case for proper nouns only
5. **Tabular numerals** for times, dates, and aligned numbers

---

## 5. Spacing & Layout

### 5.1 Spacing Scale

A consistent 4px base unit creates rhythm and alignment.

| Token | Value | Usage |
|-------|-------|-------|
| `--space-0` | 0 | None |
| `--space-1` | 4px | Tight gaps, icon padding |
| `--space-2` | 8px | Related element gaps |
| `--space-3` | 12px | Compact padding |
| `--space-4` | 16px | Standard padding |
| `--space-5` | 20px | Card padding (mobile) |
| `--space-6` | 24px | Section gaps |
| `--space-8` | 32px | Card padding (desktop) |
| `--space-10` | 40px | Large section gaps |
| `--space-12` | 48px | Page margins (display) |
| `--space-16` | 64px | Major section breaks |
| `--space-20` | 80px | Display safe areas |

### 5.2 Touch Targets

All interactive elements must meet minimum touch target requirements.

| Size | Value | Usage |
|------|-------|-------|
| **Minimum** | 44 × 44px | Smallest tappable area |
| **Comfortable** | 48 × 48px | Standard buttons |
| **Large** | 56 × 56px | Primary actions, display UI |
| **Extra Large** | 64 × 64px | Glanceable display actions |

```css
:root {
  --touch-min: 2.75rem;   /* 44px */
  --touch-md: 3rem;       /* 48px */
  --touch-lg: 3.5rem;     /* 56px */
  --touch-xl: 4rem;       /* 64px */
}
```

### 5.3 Layout Grid

#### Display Application (Portrait 1080×1920)

```
┌─────────────────────────────────────────┐
│              Safe Area (48px)           │
│  ┌───────────────────────────────────┐  │
│  │         Header Zone (200px)       │  │
│  ├───────────────────────────────────┤  │
│  │                                   │  │
│  │                                   │  │
│  │         Content Zone              │  │
│  │         (Flexible)                │  │
│  │                                   │  │
│  │                                   │  │
│  ├───────────────────────────────────┤  │
│  │         Footer Zone (120px)       │  │
│  └───────────────────────────────────┘  │
│              Safe Area (48px)           │
└─────────────────────────────────────────┘

Content Area: 984px wide (1080 - 96px margins)
Column Grid: 12 columns, 16px gutter
```

#### Web Application

```css
.container {
  --container-sm: 640px;
  --container-md: 768px;
  --container-lg: 1024px;
  --container-xl: 1280px;
}
```

#### Mobile Applications

- Full-width with safe area insets
- 16px horizontal padding
- Respect system navigation areas

### 5.4 Card Layout

Standard card padding and spacing.

```css
.card {
  padding: var(--space-5);        /* 20px mobile */
  border-radius: var(--radius-xl); /* 16px */
  gap: var(--space-4);            /* 16px between items */
}

@media (min-width: 768px) {
  .card {
    padding: var(--space-6);      /* 24px tablet+ */
  }
}

@media (min-width: 1024px) {
  .card {
    padding: var(--space-8);      /* 32px desktop/display */
  }
}
```

---

## 6. Elevation & Depth

### 6.1 Shadow Scale

Shadows create depth and visual hierarchy. They're warm-tinted to match the canvas.

| Level | Shadow | Usage |
|-------|--------|-------|
| **None** | `none` | Flat elements, inline content |
| **XS** | `0 1px 2px rgba(0,0,0,0.04)` | Subtle lift, hover states |
| **SM** | `0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04)` | Cards, buttons |
| **MD** | `0 4px 6px rgba(0,0,0,0.05), 0 2px 4px rgba(0,0,0,0.04)` | Raised cards, dropdowns |
| **LG** | `0 10px 15px rgba(0,0,0,0.06), 0 4px 6px rgba(0,0,0,0.04)` | Modals, popovers |
| **XL** | `0 20px 25px rgba(0,0,0,0.08), 0 8px 10px rgba(0,0,0,0.04)` | Dialogs, overlays |

```css
:root {
  --shadow-xs: 0 1px 2px rgba(0,0,0,0.04);
  --shadow-sm: 0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04);
  --shadow-md: 0 4px 6px rgba(0,0,0,0.05), 0 2px 4px rgba(0,0,0,0.04);
  --shadow-lg: 0 10px 15px rgba(0,0,0,0.06), 0 4px 6px rgba(0,0,0,0.04);
  --shadow-xl: 0 20px 25px rgba(0,0,0,0.08), 0 8px 10px rgba(0,0,0,0.04);
}
```

### 6.2 Border Radius

Rounded corners create warmth and approachability.

| Token | Value | Usage |
|-------|-------|-------|
| `--radius-sm` | 6px | Small elements, tags |
| `--radius-md` | 8px | Buttons, inputs |
| `--radius-lg` | 12px | Cards, containers |
| `--radius-xl` | 16px | Large cards, modals |
| `--radius-2xl` | 24px | Feature cards, dialogs |
| `--radius-full` | 9999px | Pills, avatars, circles |

### 6.3 Borders

Subtle borders define boundaries without visual noise.

```css
:root {
  --border-color: #E5E7EB;
  --border-color-light: #F3F4F6;
  --border-color-strong: #D1D5DB;
  --border-width: 1px;
}
```

---

## 7. Iconography

### 7.1 Icon Style

Luminous uses a consistent line-based icon style throughout the interface.

**Characteristics**:
- Stroke width: 1.5px (regular), 2px (emphasis)
- Rounded caps and joins
- Consistent optical sizing
- Simple, recognizable forms

### 7.2 Icon Sizes

| Size | Value | Stroke | Usage |
|------|-------|--------|-------|
| **XS** | 16px | 1.5px | Inline text, metadata |
| **SM** | 20px | 1.5px | Button icons, list items |
| **MD** | 24px | 1.5px | Standard actions |
| **LG** | 32px | 2px | Navigation, emphasis |
| **XL** | 48px | 2px | Display icons |
| **2XL** | 64px | 2px | Illustrations, empty states |

### 7.3 Icon Color

- **Default**: Text Secondary (`#6B7280`)
- **Interactive**: Brand Accent (`#0EA5E9`)
- **On Color**: White or contextual
- **Semantic**: Match status colors

### 7.4 Required Icon Set

Minimum icons needed for Luminous functionality:

**Navigation**: home, calendar, tasks, routines, rewards, lists, meals, settings, back, menu
**Actions**: add, edit, delete, check, close, search, filter, sort, share, more
**Status**: success, warning, error, info, pending, overdue
**Time**: clock, alarm, timer, sunrise, sunset, calendar-day
**Family**: user, users, child, home
**Tasks**: checkbox, circle, star, flag, tag
**Misc**: weather icons, notification, bell, camera, photo

---

## 8. UI Components

### 8.1 Buttons

#### Variants

| Variant | Background | Text | Border | Usage |
|---------|------------|------|--------|-------|
| **Primary** | `#0284C7` | White | None | Main actions |
| **Secondary** | `#F3F4F6` | `#374151` | None | Secondary actions |
| **Ghost** | Transparent | `#374151` | None | Tertiary, in-context |
| **Danger** | `#DC2626` | White | None | Destructive actions |
| **Outline** | Transparent | `#0284C7` | `#0284C7` | Alternative primary |

#### Sizes

| Size | Height | Padding | Font Size | Icon |
|------|--------|---------|-----------|------|
| **SM** | 36px | 12px 16px | 14px | 16px |
| **MD** | 44px | 14px 20px | 16px | 20px |
| **LG** | 52px | 16px 24px | 18px | 24px |
| **XL** | 60px | 18px 32px | 20px | 24px |

#### States

```css
.button-primary {
  background: var(--accent-600);
  transition: all 150ms ease;
}

.button-primary:hover {
  background: var(--accent-700);
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
}

.button-primary:active {
  background: var(--accent-800);
  transform: translateY(0);
  box-shadow: var(--shadow-sm);
}

.button-primary:focus-visible {
  outline: 2px solid var(--accent-500);
  outline-offset: 2px;
}

.button-primary:disabled {
  background: var(--gray-300);
  color: var(--gray-500);
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}
```

#### Button Rules

1. Minimum width: 64px (prevents narrow buttons)
2. Icon-only buttons must have aria-label
3. Loading state shows spinner, maintains width
4. One primary button per view/section maximum

### 8.2 Cards

The primary container for content throughout Luminous.

#### Base Card

```css
.card {
  background: var(--surface-primary);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-sm);
  padding: var(--space-5);
  border: 1px solid var(--border-color-light);
}
```

#### Card Variants

| Variant | Modifications | Usage |
|---------|---------------|-------|
| **Default** | Base styling | General content |
| **Elevated** | `shadow-md`, no border | Important content, floating |
| **Interactive** | Hover state with lift | Clickable cards |
| **Colored** | Light tint background, colored left border | Personal items |
| **Outlined** | No shadow, 1px border | Lists, nested content |

#### Interactive Card

```css
.card-interactive {
  cursor: pointer;
  transition: all 200ms ease;
}

.card-interactive:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-2px);
  border-color: var(--border-color);
}

.card-interactive:active {
  transform: translateY(0);
  box-shadow: var(--shadow-sm);
}
```

#### Personal Card (Family Member)

```css
.card-personal {
  background: var(--member-color-light);
  border-left: 4px solid var(--member-color);
  border-radius: var(--radius-lg);
}
```

### 8.3 Inputs

#### Text Input

```css
.input {
  height: 44px;
  padding: 0 var(--space-4);
  font-size: 16px; /* Prevents iOS zoom */
  background: var(--surface-primary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  transition: all 150ms ease;
}

.input:hover {
  border-color: var(--border-color-strong);
}

.input:focus {
  outline: none;
  border-color: var(--accent-500);
  box-shadow: 0 0 0 3px var(--accent-100);
}

.input:disabled {
  background: var(--surface-secondary);
  color: var(--text-tertiary);
  cursor: not-allowed;
}

.input-error {
  border-color: var(--danger-500);
}

.input-error:focus {
  box-shadow: 0 0 0 3px var(--danger-100);
}
```

#### Input Sizes

| Size | Height | Font | Usage |
|------|--------|------|-------|
| **SM** | 36px | 14px | Compact forms |
| **MD** | 44px | 16px | Default |
| **LG** | 52px | 18px | Display UI |

#### Input Addons

- **Prefix/Suffix Icons**: 20px, muted color, inside padding
- **Prefix/Suffix Text**: Same as input text, separated by light border
- **Clear Button**: Appears when input has value

### 8.4 Avatars

Family member identification throughout the interface.

#### Structure

```css
.avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--member-color);
  color: white;
  font-weight: 600;
  border-radius: var(--radius-full);
  overflow: hidden;
}
```

#### Sizes

| Size | Dimensions | Font | Usage |
|------|------------|------|-------|
| **XS** | 24px | 10px | Dense lists |
| **SM** | 32px | 12px | Inline mentions |
| **MD** | 40px | 14px | List items |
| **LG** | 56px | 18px | Cards, headers |
| **XL** | 80px | 24px | Profile pages |
| **2XL** | 120px | 36px | Display profile |

#### Avatar Content Priority

1. Profile photo (if available)
2. Initials (first letter of first name, first letter of last name)
3. Single initial (first letter) for young children

#### Avatar Group

When showing multiple members:
- Overlap by 25% of width
- Maximum 4 visible, then "+N" indicator
- Z-index: first avatar on top

### 8.5 Checkboxes & Toggles

#### Checkbox

```css
.checkbox {
  width: 24px;
  height: 24px;
  border: 2px solid var(--border-color-strong);
  border-radius: var(--radius-sm);
  transition: all 150ms ease;
}

.checkbox:checked {
  background: var(--accent-600);
  border-color: var(--accent-600);
}

.checkbox:focus-visible {
  outline: 2px solid var(--accent-500);
  outline-offset: 2px;
}
```

#### Task Checkbox (Personal)

```css
.checkbox-personal {
  border-color: var(--member-color);
}

.checkbox-personal:checked {
  background: var(--member-color);
  border-color: var(--member-color);
}
```

#### Toggle Switch

```css
.toggle {
  width: 52px;
  height: 32px;
  background: var(--gray-300);
  border-radius: var(--radius-full);
  padding: 4px;
  transition: background 200ms ease;
}

.toggle:checked {
  background: var(--accent-600);
}

.toggle-knob {
  width: 24px;
  height: 24px;
  background: white;
  border-radius: var(--radius-full);
  box-shadow: var(--shadow-sm);
  transition: transform 200ms ease;
}

.toggle:checked .toggle-knob {
  transform: translateX(20px);
}
```

### 8.6 Selection & Pills

#### Pill / Tag

```css
.pill {
  display: inline-flex;
  align-items: center;
  height: 28px;
  padding: 0 var(--space-3);
  font-size: 13px;
  font-weight: 500;
  background: var(--surface-secondary);
  border-radius: var(--radius-full);
  gap: var(--space-1);
}

.pill-colored {
  background: var(--member-color-light);
  color: var(--member-color-dark);
}
```

#### Chip (Selectable)

```css
.chip {
  height: 36px;
  padding: 0 var(--space-4);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-full);
  cursor: pointer;
  transition: all 150ms ease;
}

.chip:hover {
  background: var(--surface-interactive);
  border-color: var(--border-color-strong);
}

.chip-selected {
  background: var(--accent-100);
  border-color: var(--accent-500);
  color: var(--accent-700);
}
```

### 8.7 Lists

#### List Item

```css
.list-item {
  display: flex;
  align-items: center;
  min-height: 56px;
  padding: var(--space-3) var(--space-4);
  gap: var(--space-3);
  border-bottom: 1px solid var(--border-color-light);
}

.list-item:last-child {
  border-bottom: none;
}

.list-item-interactive {
  cursor: pointer;
  transition: background 100ms ease;
}

.list-item-interactive:hover {
  background: var(--surface-interactive);
}

.list-item-interactive:active {
  background: var(--surface-pressed);
}
```

### 8.8 Modals & Dialogs

#### Modal Overlay

```css
.modal-overlay {
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
}
```

#### Modal Container

```css
.modal {
  background: var(--surface-primary);
  border-radius: var(--radius-2xl);
  box-shadow: var(--shadow-xl);
  max-width: 480px;
  width: calc(100% - var(--space-8));
  max-height: calc(100vh - var(--space-16));
  overflow: hidden;
}

.modal-header {
  padding: var(--space-5) var(--space-6);
  border-bottom: 1px solid var(--border-color-light);
}

.modal-body {
  padding: var(--space-6);
  overflow-y: auto;
}

.modal-footer {
  padding: var(--space-4) var(--space-6);
  border-top: 1px solid var(--border-color-light);
  display: flex;
  justify-content: flex-end;
  gap: var(--space-3);
}
```

### 8.9 Alerts & Notifications

#### Inline Alert

```css
.alert {
  display: flex;
  padding: var(--space-4);
  border-radius: var(--radius-lg);
  gap: var(--space-3);
}

.alert-info {
  background: var(--info-light);
  color: var(--info-dark);
}

.alert-success {
  background: var(--success-light);
  color: var(--success-dark);
}

.alert-warning {
  background: var(--warning-light);
  color: var(--warning-dark);
}

.alert-danger {
  background: var(--danger-light);
  color: var(--danger-dark);
}
```

#### Toast Notification

```css
.toast {
  display: flex;
  align-items: center;
  padding: var(--space-4) var(--space-5);
  background: var(--gray-900);
  color: white;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  gap: var(--space-3);
}
```

### 8.10 Progress Indicators

#### Linear Progress

```css
.progress-track {
  height: 8px;
  background: var(--surface-secondary);
  border-radius: var(--radius-full);
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--accent-500);
  border-radius: var(--radius-full);
  transition: width 300ms ease;
}

.progress-fill-personal {
  background: var(--member-color);
}
```

#### Circular Spinner

```css
.spinner {
  width: 24px;
  height: 24px;
  border: 3px solid var(--surface-secondary);
  border-top-color: var(--accent-500);
  border-radius: var(--radius-full);
  animation: spin 800ms linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
```

---

## 9. Patterns & Templates

### 9.1 Page Structure

#### Display View (Portrait Screen)

```
┌────────────────────────────────────────────────┐
│                                                │
│   ┌────────────────────────────────────────┐   │
│   │  Time & Date          ☀️ 72°           │   │  Header: 180px
│   │  Thursday, January 9                   │   │
│   └────────────────────────────────────────┘   │
│                                                │
│   ┌────────────────────────────────────────┐   │
│   │                                        │   │
│   │  Today's Schedule                      │   │  Primary Card
│   │  ─────────────────────────────────     │   │  (Flexible)
│   │  • 8:00   School drop-off              │   │
│   │  • 3:30   Soccer (Liam)                │   │
│   │  • 4:00   Dentist (Emma)               │   │
│   │  • 6:00   Family dinner                │   │
│   │                                        │   │
│   └────────────────────────────────────────┘   │
│                                                │
│   ┌──────────────┐  ┌──────────────┐           │
│   │ 🔵 Liam      │  │ 🟢 Emma      │           │  Task Cards
│   │   Vacuum     │  │   Dishes     │           │  (160px each)
│   │   ○ ○ ○      │  │   ○ ○        │           │
│   └──────────────┘  └──────────────┘           │
│                                                │
│   ┌────────────────────────────────────────┐   │
│   │  Family Photo / Countdown / Weather    │   │  Footer Widget
│   └────────────────────────────────────────┘   │  (120px)
│                                                │
└────────────────────────────────────────────────┘
```

#### Mobile View

```
┌─────────────────────────┐
│  ← Calendar    +        │  Navigation Bar: 56px
├─────────────────────────┤
│                         │
│  Thursday, Jan 9        │  Page Header: 80px
│                         │
├─────────────────────────┤
│                         │
│  ┌───────────────────┐  │
│  │ Event Card 1      │  │  Content Area
│  └───────────────────┘  │  (Scrollable)
│                         │
│  ┌───────────────────┐  │
│  │ Event Card 2      │  │
│  └───────────────────┘  │
│                         │
│  ┌───────────────────┐  │
│  │ Event Card 3      │  │
│  └───────────────────┘  │
│                         │
├─────────────────────────┤
│  🏠  📅  ✓  👤  ⚙️       │  Tab Bar: 56px + safe area
└─────────────────────────┘
```

### 9.2 Event Card Pattern

For calendar events, tasks, and scheduled items.

```
┌─────────────────────────────────────────────────┐
│ ▌                                               │  4px colored left border
│ ▌  ● 3:30 PM                        Soccer ⚽   │  (family member color)
│ ▌    Riverside Park                             │
│ ▌    ○ Liam  ○ Dad                              │  Assigned members
│ ▌                                               │
└─────────────────────────────────────────────────┘
```

### 9.3 Task Item Pattern

For chores, to-dos, and checklist items.

```
Uncompleted:
┌─────────────────────────────────────────────────┐
│                                                 │
│  ○   Vacuum living room              🔵 Liam   │
│      Due today                                  │
│                                                 │
└─────────────────────────────────────────────────┘

Completed:
┌─────────────────────────────────────────────────┐
│                                                 │
│  ✓   Vacuum living room              🔵 Liam   │  Muted styling
│      Completed at 2:30 PM                       │  Strikethrough title
│                                                 │
└─────────────────────────────────────────────────┘
```

### 9.4 Empty State Pattern

When there's no content to display.

```
┌─────────────────────────────────────────────────┐
│                                                 │
│                                                 │
│                    [Icon]                       │  64px illustration icon
│                                                 │
│              All caught up!                     │  Title: 20px semibold
│                                                 │
│        No tasks scheduled for today.            │  Body: 16px secondary
│              Enjoy your free time.              │
│                                                 │
│               [ Add a task ]                    │  Optional action
│                                                 │
│                                                 │
└─────────────────────────────────────────────────┘
```

### 9.5 Profile Selector Pattern

For choosing family members.

```
┌─────────────────────────────────────────────────┐
│                                                 │
│  Who is this for?                               │
│                                                 │
│  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐        │
│  │ 🔵   │  │ 🟢   │  │ 🟠   │  │ 🟣   │        │  Avatar circles
│  │ Liam │  │ Emma │  │ Dad  │  │ Mom  │        │  with names below
│  └──────┘  └──────┘  └──────┘  └──────┘        │
│     ✓                                           │  Selection indicator
│                                                 │
└─────────────────────────────────────────────────┘
```

---

## 10. Motion & Animation

### 10.1 Motion Principles

1. **Purposeful**: Animation serves function, not decoration
2. **Swift**: Interactions feel responsive and snappy
3. **Calm**: No bouncing, shaking, or attention-grabbing movement
4. **Consistent**: Same actions have same animations throughout

### 10.2 Duration Scale

| Duration | Value | Usage |
|----------|-------|-------|
| **Instant** | 0ms | Color changes on press |
| **Quick** | 100ms | Micro-interactions, hovers |
| **Standard** | 200ms | Most transitions |
| **Moderate** | 300ms | Complex transitions |
| **Slow** | 400ms | Page transitions, modals |
| **Deliberate** | 600ms | Celebration moments |

### 10.3 Easing Functions

```css
:root {
  /* Standard motion */
  --ease-in: cubic-bezier(0.4, 0, 1, 1);
  --ease-out: cubic-bezier(0, 0, 0.2, 1);
  --ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);

  /* Emphasized motion */
  --ease-spring: cubic-bezier(0.34, 1.56, 0.64, 1);

  /* Default for most transitions */
  --ease-default: var(--ease-out);
}
```

| Easing | When to Use |
|--------|-------------|
| **ease-out** | Elements entering, appearing |
| **ease-in** | Elements leaving, disappearing |
| **ease-in-out** | Elements moving, transforming |
| **ease-spring** | Celebration moments, completion |

### 10.4 Standard Animations

#### Fade In/Out

```css
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes fadeOut {
  from { opacity: 1; }
  to { opacity: 0; }
}

.fade-enter {
  animation: fadeIn 200ms var(--ease-out);
}

.fade-exit {
  animation: fadeOut 150ms var(--ease-in);
}
```

#### Slide Up (Modal, Drawer)

```css
@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(16px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.slide-up-enter {
  animation: slideUp 300ms var(--ease-out);
}
```

#### Scale (Popover, Dropdown)

```css
@keyframes scaleIn {
  from {
    opacity: 0;
    transform: scale(0.95);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.scale-enter {
  animation: scaleIn 200ms var(--ease-out);
}
```

#### Task Completion

```css
@keyframes taskComplete {
  0% { transform: scale(1); }
  50% { transform: scale(1.1); }
  100% { transform: scale(1); }
}

.task-complete {
  animation: taskComplete 400ms var(--ease-spring);
}

/* Checkmark draw animation */
@keyframes checkDraw {
  from { stroke-dashoffset: 24; }
  to { stroke-dashoffset: 0; }
}

.check-animate {
  stroke-dasharray: 24;
  animation: checkDraw 300ms var(--ease-out) forwards;
}
```

### 10.5 Transitions Reference

| Element | Property | Duration | Easing |
|---------|----------|----------|--------|
| Button hover | background, transform | 150ms | ease-out |
| Button press | transform, shadow | 100ms | ease-in |
| Card hover | transform, shadow | 200ms | ease-out |
| Input focus | border, shadow | 150ms | ease-out |
| Checkbox | background, border | 150ms | ease-out |
| Toggle | background | 200ms | ease-in-out |
| Toggle knob | transform | 200ms | ease-spring |
| Modal open | opacity, transform | 300ms | ease-out |
| Modal close | opacity, transform | 200ms | ease-in |
| Toast enter | transform, opacity | 300ms | ease-spring |
| Toast exit | opacity | 200ms | ease-in |
| Page transition | opacity | 200ms | ease-in-out |

### 10.6 Reduced Motion

Respect user preferences for reduced motion.

```css
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

---

## 11. UX Principles & Flows

### 11.1 Core UX Principles

#### Principle 1: Zero Learning Curve

**Implementation**:
- Use universally understood icons and patterns
- Label everything (no icon-only buttons for primary actions)
- Show, don't tell (visual examples over instructions)
- Consistent placement of actions across all views

#### Principle 2: Forgiving Interactions

**Implementation**:
- Undo available for destructive actions
- Confirmation dialogs for irreversible changes
- Generous touch targets (44px minimum)
- Accidental tap protection (require deliberate action)

#### Principle 3: Immediate Feedback

**Implementation**:
- Visual response within 100ms of any tap
- Loading states for any operation >300ms
- Success/error states clearly communicated
- Progress indication for multi-step operations

#### Principle 4: Minimal Depth

**Implementation**:
- Maximum 3 levels of navigation depth
- Clear back/exit paths from any screen
- No dead ends or trapped states
- Modal dialogs for focused tasks, not navigation

#### Principle 5: Glanceable First

**Implementation**:
- Most important information largest/first
- Progressive disclosure of details
- Color-coding for quick identification
- Clear visual hierarchy in all views

### 11.2 Information Hierarchy

For any screen, content should follow this priority:

1. **What's happening now** (current time, active events)
2. **What's next** (upcoming in next few hours)
3. **What needs attention** (overdue, urgent)
4. **Everything else** (future items, completed)

### 11.3 Standard Flow Patterns

#### Creation Flow

```
[+ Button] → [Form Sheet/Modal] → [Save] → [Success Feedback] → [View Item]
                    ↓
              [Cancel] → [Confirm if dirty] → [Return]
```

**Guidelines**:
- Form appears as modal or sheet, not new page
- Save button clearly labeled with action ("Add Event", not "Save")
- Success confirmation is brief (toast or inline)
- Return to context where user started

#### Editing Flow

```
[Item] → [Edit Mode] → [Make Changes] → [Save] → [Success] → [View Mode]
              ↓
        [Cancel] → [Discard confirmation if dirty] → [View Mode]
```

**Guidelines**:
- Edit mode clearly indicated (different styling or "Editing" label)
- Auto-save where appropriate (notes, descriptions)
- Explicit save for important data (events, tasks)
- Show what changed after save

#### Deletion Flow

```
[Delete Action] → [Confirmation Dialog] → [Confirm] → [Item Removed] → [Undo Toast]
                         ↓
                   [Cancel] → [Return]
```

**Guidelines**:
- Require confirmation for all deletions
- Show what will be deleted in confirmation
- Provide undo option (5-10 seconds)
- Use red/danger styling for delete buttons

#### Selection Flow (Assigning Members)

```
[Open Selector] → [Member Avatars Grid] → [Tap to Toggle] → [Done/Auto-close]
```

**Guidelines**:
- Multi-select by default for most contexts
- Visual checkmark on selected items
- Allow deselect by tapping again
- For single-select, close on selection

### 11.4 Navigation Patterns

#### Display Application

- **Primary Navigation**: Swipe or tap to cycle between main views
- **Views**: Schedule → Tasks → Routines → (Widgets)
- **No deep navigation** on display—it's a dashboard, not an app

#### Mobile Application

- **Tab Bar**: 5 core sections maximum
  - Home (dashboard)
  - Calendar
  - Tasks
  - Family
  - Settings

- **Stack Navigation**: Push/pop within each tab
- **Modals**: For creation/editing flows
- **Back Button**: Always available when depth > 1

#### Web Application

- **Sidebar Navigation**: Persistent on desktop
- **Mobile**: Collapsible to hamburger
- **Breadcrumbs**: For deep navigation paths

### 11.5 Loading States

| Duration | Response |
|----------|----------|
| 0-100ms | No loading indicator |
| 100-300ms | Subtle opacity change or skeleton |
| 300ms-2s | Spinner or skeleton screen |
| 2s+ | Progress bar or detailed status |

#### Skeleton Screens

Use skeleton screens for content-heavy views:

```css
.skeleton {
  background: linear-gradient(
    90deg,
    var(--surface-secondary) 25%,
    var(--surface-interactive) 50%,
    var(--surface-secondary) 75%
  );
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: var(--radius-md);
}

@keyframes shimmer {
  0% { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}
```

### 11.6 Error Handling

| Error Type | Presentation | Action |
|------------|--------------|--------|
| **Field Validation** | Red border + inline message | Focus field |
| **Form Submission** | Alert at top of form | Highlight all errors |
| **Network Error** | Toast notification | Retry button |
| **Critical Error** | Full-screen message | Clear recovery path |

**Error Message Guidelines**:
- State what happened (not technical details)
- Suggest what to do next
- Never blame the user
- Provide a clear action to recover

Good: "Couldn't save. Check your connection and try again."
Bad: "Error 500: Internal Server Exception"

### 11.7 Success States

| Action | Feedback | Duration |
|--------|----------|----------|
| **Task Complete** | Checkmark animation + color | 400ms |
| **Item Created** | "Added" toast + show item | 2s toast |
| **Item Updated** | "Saved" toast | 2s toast |
| **Item Deleted** | "Deleted" toast with undo | 5s toast |

---

## 12. Accessibility

### 12.1 Compliance Target

**WCAG 2.1 Level AA** — all interfaces must meet this standard.

### 12.2 Color & Contrast

| Element | Minimum Contrast |
|---------|------------------|
| Body text | 4.5:1 |
| Large text (18px+ or 14px+ bold) | 3:1 |
| UI components | 3:1 |
| Focus indicators | 3:1 |
| Non-text graphics | 3:1 |

**Implementation**:
- Test all color combinations with contrast checker
- Don't rely on color alone to convey information
- Provide text labels alongside color indicators

### 12.3 Touch & Motor

| Requirement | Specification |
|-------------|---------------|
| Touch target size | 44 × 44px minimum |
| Touch target spacing | 8px minimum between targets |
| Gesture alternatives | All gestures have tap alternatives |
| Timeout | Warn before session timeout, allow extension |

### 12.4 Visual

| Requirement | Specification |
|-------------|---------------|
| Text resize | Support up to 200% without loss |
| Zoom | Support up to 400% zoom |
| Motion | Respect prefers-reduced-motion |
| Focus visible | 2px outline, 2px offset, visible color |

### 12.5 Screen Readers

**Required Practices**:
- Semantic HTML elements (button, nav, main, etc.)
- Proper heading hierarchy (h1 → h2 → h3)
- Alt text for all meaningful images
- aria-label for icon-only buttons
- aria-live regions for dynamic content
- Role and state for custom components

### 12.6 Keyboard Navigation

| Requirement | Implementation |
|-------------|----------------|
| All interactive elements focusable | tabindex="0" where needed |
| Logical focus order | Follow visual reading order |
| No focus traps | Except modals (with escape exit) |
| Skip links | "Skip to main content" on web |
| Keyboard shortcuts | Optional, not required |

### 12.7 Accessible Alternatives

| Mode | Trigger | Changes |
|------|---------|---------|
| **Large Text** | User setting | +25% base font size |
| **High Contrast** | User setting | Enhanced borders, stronger shadows |
| **Dyslexia-Friendly** | User setting | OpenDyslexic font option |

---

## 13. Personalization

### 13.1 Family Member Colors

Each household member is assigned a personal color from the 8-color palette. This color identifies their items throughout the interface.

#### Color Assignment

**Initial Setup**:
1. First member gets Sky (default)
2. Subsequent members get next available color
3. System avoids recently used colors for variety
4. Users may change their color at any time

**Color in Use**:
- Avatar background
- Task card left border + light tint
- Calendar event indicator
- Routine assignments
- Reward/completion celebrations

### 13.2 Theme Preferences

#### Canvas Warmth

Users can adjust the base canvas temperature:

| Setting | Description |
|---------|-------------|
| **Auto** | Follows time-based adaptation (default) |
| **Cool** | Fixed cool white canvas |
| **Warm** | Fixed warm cream canvas |
| **Night Display** | Dark mode (optional) |

#### Personalization per Household

These settings apply to the entire household:

- Canvas warmth preference
- Night Display hours (if enabled)
- Time format (12h / 24h)
- Date format (regional)
- First day of week

### 13.3 Display Configuration

The wall-mounted display can be configured:

| Setting | Options |
|---------|---------|
| **Default View** | Schedule / Tasks / Routines |
| **Auto-rotate** | Cycle views every N minutes |
| **Brightness** | Manual or auto (with sensor) |
| **Screen timeout** | Sleep schedule |
| **Orientation** | Portrait (default) / Landscape |

---

## 14. Platform Adaptations

### 14.1 Display Application (Electron)

**Optimizations**:
- Larger touch targets (56px standard)
- Display-optimized font sizes (see Typography)
- No hover states (touch only)
- High contrast by default
- Reduced animation for performance
- Always-on considerations (minimal GPU usage)

**Unique Patterns**:
- Full-screen by default (kiosk mode)
- Swipe navigation between views
- Long-press for actions (vs tap)
- No keyboard input expected

### 14.2 Web Application (Angular)

**Responsive Breakpoints**:
```css
--breakpoint-sm: 640px;   /* Large phone */
--breakpoint-md: 768px;   /* Tablet */
--breakpoint-lg: 1024px;  /* Small desktop */
--breakpoint-xl: 1280px;  /* Desktop */
--breakpoint-2xl: 1536px; /* Large desktop */
```

**Adaptations**:
- Sidebar navigation on desktop, tab bar on mobile
- Hover states enabled
- Keyboard shortcuts available
- Standard browser scroll behavior

### 14.3 iOS Application (SwiftUI)

**Native Patterns**:
- Use SF Symbols for icons
- Respect Safe Area insets
- Native navigation (UINavigationController)
- Haptic feedback on actions
- iOS-style sheets and alerts

**Color Adjustments**:
- Use iOS semantic colors where appropriate
- Support Dynamic Type
- Support Dark Mode (matches Night Display)

### 14.4 Android Application (Compose)

**Native Patterns**:
- Material Design 3 components where sensible
- Navigation rail on tablets
- Android-style FAB for primary action
- System navigation bar handling

**Color Adjustments**:
- Support Material You dynamic color (optional)
- Support Dark theme
- Handle edge-to-edge display

---

## 15. Implementation Reference

### 15.1 CSS Custom Properties

Complete reference of all design tokens as CSS custom properties.

```css
:root {
  /* Canvas (time-adaptive) */
  --canvas: #FDFCFA;

  /* Surfaces */
  --surface-primary: #FFFFFF;
  --surface-secondary: #FAFAFA;
  --surface-interactive: #F9FAFB;
  --surface-pressed: #F3F4F6;

  /* Text */
  --text-primary: #111827;
  --text-secondary: #6B7280;
  --text-tertiary: #9CA3AF;
  --text-inverse: #FFFFFF;

  /* Borders */
  --border-color: #E5E7EB;
  --border-color-light: #F3F4F6;
  --border-color-strong: #D1D5DB;

  /* Brand accent */
  --accent-50: #F0F9FF;
  --accent-100: #E0F2FE;
  --accent-200: #BAE6FD;
  --accent-300: #7DD3FC;
  --accent-400: #38BDF8;
  --accent-500: #0EA5E9;
  --accent-600: #0284C7;
  --accent-700: #0369A1;
  --accent-800: #075985;
  --accent-900: #0C4A6E;

  /* Semantic */
  --success: #16A34A;
  --success-light: #DCFCE7;
  --success-dark: #166534;

  --warning: #D97706;
  --warning-light: #FEF3C7;
  --warning-dark: #B45309;

  --danger: #DC2626;
  --danger-light: #FEE2E2;
  --danger-dark: #B91C1C;

  --info: #0284C7;
  --info-light: #E0F2FE;
  --info-dark: #075985;

  /* Family colors */
  --member-sky: #0EA5E9;
  --member-emerald: #10B981;
  --member-amber: #F59E0B;
  --member-orange: #F97316;
  --member-rose: #F43F5E;
  --member-violet: #8B5CF6;
  --member-pink: #EC4899;
  --member-teal: #14B8A6;

  /* Spacing */
  --space-1: 4px;
  --space-2: 8px;
  --space-3: 12px;
  --space-4: 16px;
  --space-5: 20px;
  --space-6: 24px;
  --space-8: 32px;
  --space-10: 40px;
  --space-12: 48px;
  --space-16: 64px;

  /* Radii */
  --radius-sm: 6px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-xl: 16px;
  --radius-2xl: 24px;
  --radius-full: 9999px;

  /* Shadows */
  --shadow-xs: 0 1px 2px rgba(0,0,0,0.04);
  --shadow-sm: 0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04);
  --shadow-md: 0 4px 6px rgba(0,0,0,0.05), 0 2px 4px rgba(0,0,0,0.04);
  --shadow-lg: 0 10px 15px rgba(0,0,0,0.06), 0 4px 6px rgba(0,0,0,0.04);
  --shadow-xl: 0 20px 25px rgba(0,0,0,0.08), 0 8px 10px rgba(0,0,0,0.04);

  /* Touch targets */
  --touch-min: 44px;
  --touch-md: 48px;
  --touch-lg: 56px;
  --touch-xl: 64px;

  /* Typography */
  --font-family: system-ui, -apple-system, BlinkMacSystemFont,
                 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;

  /* Motion */
  --duration-instant: 0ms;
  --duration-quick: 100ms;
  --duration-standard: 200ms;
  --duration-moderate: 300ms;
  --duration-slow: 400ms;
  --duration-deliberate: 600ms;

  --ease-in: cubic-bezier(0.4, 0, 1, 1);
  --ease-out: cubic-bezier(0, 0, 0.2, 1);
  --ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
  --ease-spring: cubic-bezier(0.34, 1.56, 0.64, 1);
}
```

### 15.2 Tailwind Configuration

Extend Tailwind with Luminous design tokens:

```javascript
// tailwind.config.js
module.exports = {
  theme: {
    extend: {
      colors: {
        canvas: 'var(--canvas)',
        surface: {
          primary: 'var(--surface-primary)',
          secondary: 'var(--surface-secondary)',
          interactive: 'var(--surface-interactive)',
          pressed: 'var(--surface-pressed)',
        },
        accent: {
          50: 'var(--accent-50)',
          100: 'var(--accent-100)',
          200: 'var(--accent-200)',
          300: 'var(--accent-300)',
          400: 'var(--accent-400)',
          500: 'var(--accent-500)',
          600: 'var(--accent-600)',
          700: 'var(--accent-700)',
          800: 'var(--accent-800)',
          900: 'var(--accent-900)',
        },
        member: {
          sky: 'var(--member-sky)',
          emerald: 'var(--member-emerald)',
          amber: 'var(--member-amber)',
          orange: 'var(--member-orange)',
          rose: 'var(--member-rose)',
          violet: 'var(--member-violet)',
          pink: 'var(--member-pink)',
          teal: 'var(--member-teal)',
        },
      },
      spacing: {
        'touch-min': 'var(--touch-min)',
        'touch-md': 'var(--touch-md)',
        'touch-lg': 'var(--touch-lg)',
        'touch-xl': 'var(--touch-xl)',
      },
      borderRadius: {
        DEFAULT: 'var(--radius-md)',
        sm: 'var(--radius-sm)',
        md: 'var(--radius-md)',
        lg: 'var(--radius-lg)',
        xl: 'var(--radius-xl)',
        '2xl': 'var(--radius-2xl)',
      },
      boxShadow: {
        xs: 'var(--shadow-xs)',
        sm: 'var(--shadow-sm)',
        md: 'var(--shadow-md)',
        lg: 'var(--shadow-lg)',
        xl: 'var(--shadow-xl)',
      },
      fontFamily: {
        sans: 'var(--font-family)',
      },
      transitionDuration: {
        instant: 'var(--duration-instant)',
        quick: 'var(--duration-quick)',
        standard: 'var(--duration-standard)',
        moderate: 'var(--duration-moderate)',
        slow: 'var(--duration-slow)',
        deliberate: 'var(--duration-deliberate)',
      },
      transitionTimingFunction: {
        'ease-in': 'var(--ease-in)',
        'ease-out': 'var(--ease-out)',
        'ease-in-out': 'var(--ease-in-out)',
        spring: 'var(--ease-spring)',
      },
    },
  },
};
```

### 15.3 Component Checklist

Use this checklist when building new components:

- [ ] Uses design system colors (no hardcoded values)
- [ ] Follows spacing scale (multiples of 4px)
- [ ] Uses correct radius token
- [ ] Touch targets meet minimum (44px)
- [ ] Hover states defined (web only)
- [ ] Focus states visible and compliant
- [ ] Loading state if async
- [ ] Error state if applicable
- [ ] Disabled state styled
- [ ] Animation uses standard durations/easing
- [ ] Respects prefers-reduced-motion
- [ ] Screen reader accessible
- [ ] Keyboard navigable
- [ ] Works on all target platforms

---

## 16. Multi-Platform Token Export

Luminous uses a **single source of truth** for design tokens that are exported to all platforms (Web, Display, iOS, Android).

### 16.1 Token Pipeline Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                       DESIGN TOKEN PIPELINE                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  design-tokens/tokens.json                                               │
│       │                                                                  │
│       ▼                                                                  │
│  Style Dictionary (build tool)                                          │
│       │                                                                  │
│       ├─────────────────┬─────────────────┬─────────────────┐           │
│       ▼                 ▼                 ▼                 ▼           │
│  CSS Variables      Swift Extensions   Kotlin Object    Tailwind        │
│  (Angular)          (iOS)              (Android)        Config          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 16.2 Token Source Format

Tokens are defined in JSON following the [Design Tokens Community Group](https://design-tokens.github.io/community-group/format/) format:

```json
{
  "color": {
    "canvas": {
      "dawn": { "$value": "#FFFCF7", "$type": "color" },
      "morning": { "$value": "#FEFDFB", "$type": "color" },
      "afternoon": { "$value": "#FDFCFA", "$type": "color" },
      "evening": { "$value": "#FDF9F3", "$type": "color" },
      "night": { "$value": "#FAF7F2", "$type": "color" }
    },
    "accent": {
      "50": { "$value": "#F0F9FF", "$type": "color" },
      "100": { "$value": "#E0F2FE", "$type": "color" },
      "500": { "$value": "#0EA5E9", "$type": "color" },
      "600": { "$value": "#0284C7", "$type": "color" },
      "700": { "$value": "#0369A1", "$type": "color" }
    },
    "member": {
      "sky": { "$value": "#0EA5E9", "$type": "color" },
      "emerald": { "$value": "#10B981", "$type": "color" },
      "amber": { "$value": "#F59E0B", "$type": "color" },
      "rose": { "$value": "#F43F5E", "$type": "color" },
      "violet": { "$value": "#8B5CF6", "$type": "color" }
    }
  },
  "spacing": {
    "1": { "$value": "4px", "$type": "dimension" },
    "2": { "$value": "8px", "$type": "dimension" },
    "4": { "$value": "16px", "$type": "dimension" },
    "8": { "$value": "32px", "$type": "dimension" }
  },
  "radius": {
    "sm": { "$value": "6px", "$type": "dimension" },
    "md": { "$value": "8px", "$type": "dimension" },
    "lg": { "$value": "12px", "$type": "dimension" },
    "xl": { "$value": "16px", "$type": "dimension" }
  },
  "touch": {
    "min": { "$value": "44px", "$type": "dimension" },
    "md": { "$value": "48px", "$type": "dimension" },
    "lg": { "$value": "56px", "$type": "dimension" }
  }
}
```

### 16.3 Platform Output Formats

#### CSS Custom Properties (Angular Web/Display)

```css
/* Generated: design-tokens/build/css/tokens.css */
:root {
  --color-canvas-dawn: #FFFCF7;
  --color-canvas-morning: #FEFDFB;
  --color-accent-500: #0EA5E9;
  --color-accent-600: #0284C7;
  --color-member-sky: #0EA5E9;
  --color-member-emerald: #10B981;
  --spacing-1: 4px;
  --spacing-4: 16px;
  --radius-md: 8px;
  --touch-min: 44px;
}
```

#### Swift Extensions (iOS)

```swift
// Generated: design-tokens/build/swift/DesignTokens.swift
import SwiftUI

public enum DesignTokens {
    public enum Color {
        public enum Canvas {
            public static let dawn = SwiftUI.Color(hex: "FFFCF7")
            public static let morning = SwiftUI.Color(hex: "FEFDFB")
        }
        public enum Accent {
            public static let _500 = SwiftUI.Color(hex: "0EA5E9")
            public static let _600 = SwiftUI.Color(hex: "0284C7")
        }
        public enum Member {
            public static let sky = SwiftUI.Color(hex: "0EA5E9")
            public static let emerald = SwiftUI.Color(hex: "10B981")
        }
    }
    public enum Spacing {
        public static let _1: CGFloat = 4
        public static let _4: CGFloat = 16
    }
    public enum Radius {
        public static let md: CGFloat = 8
    }
    public enum Touch {
        public static let min: CGFloat = 44
    }
}
```

#### Kotlin Constants (Android)

```kotlin
// Generated: design-tokens/build/kotlin/DesignTokens.kt
package com.luminous.design

import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp

object DesignTokens {
    object Colors {
        object Canvas {
            val dawn = Color(0xFFFFFCF7)
            val morning = Color(0xFFFEFDFB)
        }
        object Accent {
            val _500 = Color(0xFF0EA5E9)
            val _600 = Color(0xFF0284C7)
        }
        object Member {
            val sky = Color(0xFF0EA5E9)
            val emerald = Color(0xFF10B981)
        }
    }
    object Spacing {
        val _1 = 4.dp
        val _4 = 16.dp
    }
    object Radius {
        val md = 8.dp
    }
    object Touch {
        val min = 44.dp
    }
}
```

### 16.4 Style Dictionary Configuration

```javascript
// design-tokens/config.js
module.exports = {
  source: ['tokens.json'],
  platforms: {
    css: {
      transformGroup: 'css',
      buildPath: 'build/css/',
      files: [{
        destination: 'tokens.css',
        format: 'css/variables',
        options: { outputReferences: true }
      }]
    },
    swift: {
      transformGroup: 'swift',
      buildPath: 'build/swift/',
      files: [{
        destination: 'DesignTokens.swift',
        format: 'swift/class.swift',
        className: 'DesignTokens'
      }]
    },
    kotlin: {
      transformGroup: 'compose',
      buildPath: 'build/kotlin/',
      files: [{
        destination: 'DesignTokens.kt',
        format: 'compose/object',
        packageName: 'com.luminous.design'
      }]
    },
    tailwind: {
      transformGroup: 'js',
      buildPath: 'build/tailwind/',
      files: [{
        destination: 'tokens.js',
        format: 'javascript/module'
      }]
    }
  }
};
```

### 16.5 CI/CD Integration

Tokens are rebuilt automatically when the source file changes:

```yaml
# .github/workflows/design-tokens.yml
name: Build Design Tokens

on:
  push:
    paths:
      - 'design-tokens/tokens.json'
      - 'design-tokens/config.js'

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Install Style Dictionary
        run: npm install -g style-dictionary

      - name: Build tokens
        run: cd design-tokens && style-dictionary build

      - name: Commit generated tokens
        run: |
          git config user.name "GitHub Actions"
          git config user.email "actions@github.com"
          git add design-tokens/build/
          git diff --staged --quiet || git commit -m "🎨 chore: Regenerate design tokens"
          git push
```

### 16.6 Platform Usage

#### Angular (Web/Display)

```typescript
// Import generated CSS in styles.css
@import '../../../design-tokens/build/css/tokens.css';

// Use in components
.button-primary {
  background: var(--color-accent-600);
  border-radius: var(--radius-md);
  min-height: var(--touch-min);
}
```

#### iOS (SwiftUI)

```swift
import SwiftUI

struct PrimaryButton: View {
    let title: String

    var body: some View {
        Text(title)
            .frame(minHeight: DesignTokens.Touch.min)
            .background(DesignTokens.Color.Accent._600)
            .cornerRadius(DesignTokens.Radius.md)
    }
}
```

#### Android (Compose)

```kotlin
import com.luminous.design.DesignTokens

@Composable
fun PrimaryButton(title: String, onClick: () -> Unit) {
    Button(
        onClick = onClick,
        modifier = Modifier.heightIn(min = DesignTokens.Touch.min),
        shape = RoundedCornerShape(DesignTokens.Radius.md),
        colors = ButtonDefaults.buttonColors(
            containerColor = DesignTokens.Colors.Accent._600
        )
    ) {
        Text(title)
    }
}
```

### 16.7 Token Categories

| Category | Description | Platforms |
|----------|-------------|-----------|
| **Colors** | Canvas, surface, text, accent, semantic, member | All |
| **Spacing** | 4px base unit scale (1-20) | All |
| **Typography** | Font sizes, weights, line heights | All |
| **Radii** | Border radius scale | All |
| **Shadows** | Elevation scale | Web, Display |
| **Motion** | Duration, easing curves | Web, Display |
| **Touch** | Minimum touch targets | All |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | January 2026 | Initial specification |
| 1.1.0 | January 2026 | Added multi-platform token export (Section 16) |

---

## Contributing

Proposed changes to the design system must:

1. Be documented in an ADR (Architecture Decision Record)
2. Include rationale tied to design principles
3. Consider impact across all platforms
4. Maintain backward compatibility or document migration
5. Be reviewed by design and engineering leads

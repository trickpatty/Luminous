# Design System Implementation Plan

> **Version**: 1.0.0
> **Created**: January 2026
> **Status**: Planning

This document outlines the comprehensive plan for migrating the Luminous web application from its current styling implementation to the new canonical Design System defined in [DESIGN-SYSTEM.md](./DESIGN-SYSTEM.md).

---

## Executive Summary

The current Angular web application uses Tailwind CSS with custom extensions but does not fully align with the new Design System specification. This plan details the changes required across colors, typography, spacing, components, animations, navigation, and overall UX to achieve complete alignment.

**Estimated Scope**: Medium-Large
**Priority Areas**: CSS Variables/Tokens, Color System, Component Updates, Navigation, Animations

---

## Gap Analysis

### 1. Color System Gaps

| Aspect | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Canvas Colors** | No time-adaptive canvas | 5 time-based canvas colors with 30-min transitions | Add adaptive canvas service + CSS variables |
| **Surface Colors** | Uses white/gray-50 | 5 semantic surface colors (`surface-primary`, `secondary`, etc.) | Create surface token system |
| **Text Colors** | Uses Tailwind grays | 5 semantic text tokens with specific hex values | Migrate to `--text-primary`, etc. |
| **Brand Accent** | Uses `primary-*` (sky blue) | Uses `accent-*` (same values, different naming) | Rename tokens for clarity |
| **Semantic Colors** | Uses default Tailwind colors | Specific success/warning/danger/info hex values | Update to design system values |
| **Family Member Colors** | 8 colors in `family` theme | 8 colors in `--member-*` format | Rename + ensure correct hex values |
| **Night Display Mode** | Not implemented | Optional dark mode with specific colors | Add opt-in dark mode support |

#### Color Migration Details

**Current Family Colors** (need migration):
```javascript
// Current (tailwind.config.js)
family: {
  blue: '#3b82f6',
  green: '#22c55e',
  yellow: '#eab308',
  orange: '#f97316',
  red: '#ef4444',
  purple: '#a855f7',
  pink: '#ec4899',
  teal: '#14b8a6',
}
```

**Target Family Colors** (design system):
```css
--member-sky: #0EA5E9;
--member-emerald: #10B981;
--member-amber: #F59E0B;
--member-orange: #F97316;
--member-rose: #F43F5E;
--member-violet: #8B5CF6;
--member-pink: #EC4899;
--member-teal: #14B8A6;
```

**Changes Needed**:
- `blue` → `sky` (#3b82f6 → #0EA5E9)
- `green` → `emerald` (#22c55e → #10B981)
- `yellow` → `amber` (#eab308 → #F59E0B)
- `red` → `rose` (#ef4444 → #F43F5E)
- `purple` → `violet` (#a855f7 → #8B5CF6)
- `orange`, `pink`, `teal` - names match, hex values match

---

### 2. Typography Gaps

| Aspect | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Font Family** | System fonts (correct) | System fonts (no change) | ✓ Already aligned |
| **Display Sizes** | 3 sizes (lg/md/sm) | 4 sizes (xl/lg/md/sm) with specific rem values | Add Display XL, update sizes |
| **Content Sizes** | Using Tailwind defaults | 8 specific content sizes (Title LG→Caption→Overline) | Create typography utility classes |
| **Font Weights** | Tailwind defaults | 4 specific weights (400/500/600/700) | Align weight usage |
| **Tabular Numbers** | Not implemented | `font-variant-numeric: tabular-nums` for times | Add utility class |
| **Glanceable Text** | `.text-glanceable` exists | Needs specific letter-spacing/smoothing | Update implementation |

#### Typography Migration Details

**Current Display Sizes**:
```javascript
'display-lg': ['4rem', { lineHeight: '1.1' }],   // 64px
'display-md': ['3rem', { lineHeight: '1.2' }],   // 48px
'display-sm': ['2rem', { lineHeight: '1.3' }],   // 32px
```

**Target Display Sizes**:
```css
Display XL: 72px / 4.5rem, line-height: 1.1, weight: 600
Display LG: 56px / 3.5rem, line-height: 1.15, weight: 600
Display MD: 40px / 2.5rem, line-height: 1.2, weight: 500
Display SM: 32px / 2rem, line-height: 1.25, weight: 500
```

---

### 3. Spacing & Layout Gaps

| Aspect | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Spacing Scale** | Tailwind defaults (4px base) | 12 specific tokens `--space-1` to `--space-20` | Add CSS variables |
| **Touch Targets** | 44px min defined | 4 sizes (44/48/56/64px) as tokens | Add touch target tokens |
| **Container Widths** | Default Tailwind | 4 specific breakpoints | Update container config |
| **Card Padding** | Fixed padding | Responsive padding (20px mobile → 32px desktop) | Update card component |
| **Layout Grid** | Not formalized | Defined zones for display (portrait 1080×1920) | Add grid system |

---

### 4. Elevation & Depth Gaps

| Aspect | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Shadow Scale** | Tailwind defaults | 5 warm-tinted shadows (`--shadow-xs` to `--shadow-xl`) | Update shadow tokens |
| **Border Radius** | Tailwind defaults | 6 specific tokens (`--radius-sm` to `--radius-full`) | Update radius tokens |
| **Borders** | Uses default colors | 3 specific border colors | Add border color tokens |

#### Shadow Migration Details

**Target Shadows** (slightly warm-tinted):
```css
--shadow-xs: 0 1px 2px rgba(0,0,0,0.04);
--shadow-sm: 0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04);
--shadow-md: 0 4px 6px rgba(0,0,0,0.05), 0 2px 4px rgba(0,0,0,0.04);
--shadow-lg: 0 10px 15px rgba(0,0,0,0.06), 0 4px 6px rgba(0,0,0,0.04);
--shadow-xl: 0 20px 25px rgba(0,0,0,0.08), 0 8px 10px rgba(0,0,0,0.04);
```

---

### 5. Component Gaps

#### 5.1 Button Component

| Aspect | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Variants** | 4 (primary/secondary/ghost/danger) | 5 (add outline) | Add outline variant |
| **Sizes** | 3 (sm/md/lg) | 4 (sm/md/lg/xl) | Add xl size |
| **Height** | Not explicit | Explicit heights (36/44/52/60px) | Update sizing |
| **Min Width** | None | 64px minimum | Add min-width |
| **Hover Effect** | Color change | Color + subtle lift + shadow | Add transform + shadow |
| **Focus Ring** | Focus visible outline | 2px outline + 2px offset | Update focus styling |

#### 5.2 Card Component

| Aspect | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Base Styling** | White + shadow | White + `--shadow-sm` + light border | Update styling |
| **Variants** | Simple/hoverable | 5 variants (default/elevated/interactive/colored/outlined) | Add variants |
| **Interactive Lift** | None | 2px lift on hover | Add transform |
| **Personal Cards** | Not implemented | Light tint + 4px colored left border | Create variant |

#### 5.3 Avatar Component

| Aspect | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Sizes** | 5 (xs-xl) | 6 (xs-2xl) | Add 2xl (120px) |
| **Color Logic** | Hash-based from name | Member-assigned color from profile | Use profile color |
| **Font Sizes** | Proportional | Specific per size | Update font mapping |
| **Avatar Groups** | Not implemented | 25% overlap, max 4 visible | Create avatar group |

#### 5.4 Input Component

| Aspect | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Height** | 44px (correct) | 44px default ✓ | Already aligned |
| **Sizes** | Single size | 3 sizes (36/44/52px) | Add size prop |
| **Focus Ring** | Blue ring | `--accent-500` with 3px spread | Update focus styling |
| **Error State** | Red border | Red border + red focus ring | Update error focus |

#### 5.5 New Components Needed

The design system defines components not yet implemented:

| Component | Priority | Description |
|-----------|----------|-------------|
| **Toggle Switch** | High | On/off toggle (52×32px) |
| **Pill/Tag** | Medium | Small labels (28px height) |
| **Chip** | Medium | Selectable pills (36px height) |
| **List Item** | High | Standard list row (56px min height) |
| **Modal** | Medium | Update existing to match spec |
| **Toast** | High | Dark background notifications |
| **Progress Bar** | Medium | Linear progress indicator |
| **Skeleton** | Medium | Loading placeholders |

---

### 6. Navigation Gaps

| Aspect | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Mobile Tab Bar** | Not implemented | 5 tabs + safe area (56px + safe) | Create mobile nav |
| **Desktop Sidebar** | Exists, functional | Update styling to match spec | Refine styling |
| **Icon Size** | Inline SVG | 24px standard, 32px for nav | Standardize icons |
| **Active States** | bg-primary-50 | Should use `--accent-100` | Update colors |
| **Max Depth** | Not enforced | 3 levels maximum | Design pattern |

---

### 7. Animation Gaps

| Aspect | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Duration Scale** | Tailwind defaults | 6 specific durations (0-600ms) | Add duration tokens |
| **Easing Functions** | Tailwind defaults | 4 custom cubic-beziers | Add easing tokens |
| **Standard Animations** | Basic transitions | 6 named animations (fadeIn, slideUp, etc.) | Create keyframes |
| **Task Completion** | None | Scale + checkmark draw animation | Create celebration |
| **Reduced Motion** | Not implemented | Media query for accessibility | Add prefers-reduced-motion |
| **Button Hover** | Color only | Color + translateY(-1px) + shadow | Update button states |
| **Card Hover** | Shadow only | translateY(-2px) + enhanced shadow | Update card states |

#### Animation Token Details

**Duration Scale**:
```css
--duration-instant: 0ms;
--duration-quick: 100ms;
--duration-standard: 200ms;
--duration-moderate: 300ms;
--duration-slow: 400ms;
--duration-deliberate: 600ms;
```

**Easing Functions**:
```css
--ease-in: cubic-bezier(0.4, 0, 1, 1);
--ease-out: cubic-bezier(0, 0, 0.2, 1);
--ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
--ease-spring: cubic-bezier(0.34, 1.56, 0.64, 1);
```

---

### 8. Dashboard Widget Gaps

| Widget | Current State | Target State | Action Required |
|--------|--------------|--------------|-----------------|
| **Time Display** | Not on display | 72px time with period adaptation | Create time widget |
| **Today's Schedule** | Card with list | Primary card with event pattern | Update event cards |
| **Task Cards** | Basic list | Personal cards with colored borders | Create task card pattern |
| **Stats Cards** | Grid of 4 | Should follow card patterns | Update styling |
| **Empty States** | Not standardized | 64px icon + title + body + action | Create empty state component |
| **Profile Selector** | Avatar grid exists | Standardize with design pattern | Update profile picker |

---

### 9. UX Flow Gaps

| Flow | Current State | Target State | Action Required |
|------|--------------|--------------|-----------------|
| **Creation Flow** | Modal forms | Modal → Save → Success Toast → View | Add success feedback |
| **Deletion Flow** | Confirmation modal | Confirmation → Delete → Undo Toast | Add undo functionality |
| **Loading States** | Spinner component | Skeleton screens for content areas | Add skeleton patterns |
| **Error Messages** | Basic display | User-friendly with recovery actions | Update error UX |

---

## Implementation Phases

### Phase 1: Design Tokens Foundation (Priority: Critical)

Create the core CSS custom properties that all components will use.

**Files to Create/Modify**:
- `clients/web/src/styles/design-tokens.scss` (new)
- `clients/web/src/styles.scss` (update)
- `clients/web/tailwind.config.js` (update)

**Tasks**:
1. Create all CSS custom properties from design system
2. Set up canvas color variables
3. Set up surface color variables
4. Set up text color variables
5. Set up accent color scale
6. Set up semantic colors (success/warning/danger/info)
7. Set up member colors
8. Set up spacing tokens
9. Set up radius tokens
10. Set up shadow tokens
11. Set up animation duration tokens
12. Set up easing function tokens
13. Update Tailwind config to reference CSS variables

---

### Phase 2: Typography System

**Files to Modify**:
- `clients/web/tailwind.config.js`
- `clients/web/src/styles.scss`

**Tasks**:
1. Update display type sizes (XL/LG/MD/SM)
2. Create content type classes (Title LG/MD/SM, Body LG/MD/SM, Caption, Overline)
3. Add tabular number utility class
4. Update `.text-glanceable` styling
5. Add `.text-time` for clock display

---

### Phase 3: Core Component Updates

#### 3.1 Button Component

**File**: `clients/web/src/app/shared/components/button/button.component.ts`

**Tasks**:
1. Add `outline` variant
2. Add `xl` size (60px height)
3. Update heights to explicit values (36/44/52/60px)
4. Add 64px minimum width
5. Add hover transform (translateY -1px) and shadow
6. Update focus ring styling (2px offset)
7. Add loading state spinner alignment

#### 3.2 Card Component

**File**: `clients/web/src/app/shared/components/card/card.component.ts`

**Tasks**:
1. Add variants: `default`, `elevated`, `interactive`, `colored`, `outlined`
2. Update base styling to use design tokens
3. Add interactive hover effect (2px lift)
4. Create personal card variant with colored border
5. Add responsive padding

#### 3.3 Avatar Component

**File**: `clients/web/src/app/shared/components/avatar/avatar.component.ts`

**Tasks**:
1. Add `2xl` size (120px, 36px font)
2. Update to use member color from profile (not hash)
3. Update font sizes per size variant
4. Create avatar group wrapper component

#### 3.4 Input Component

**File**: `clients/web/src/app/shared/components/input/input.component.ts`

**Tasks**:
1. Add size prop (`sm`/`md`/`lg`)
2. Update focus ring to use accent color
3. Update error focus ring styling

---

### Phase 4: New Components

#### 4.1 Toggle Component

**File**: `clients/web/src/app/shared/components/toggle/toggle.component.ts` (new)

**Tasks**:
1. Create toggle switch component (52×32px)
2. Implement checked/unchecked states
3. Add transition animation
4. Implement ControlValueAccessor

#### 4.2 Pill/Tag Component

**File**: `clients/web/src/app/shared/components/pill/pill.component.ts` (new)

**Tasks**:
1. Create pill component (28px height)
2. Add colored variant for member pills
3. Add removable variant with X button

#### 4.3 List Item Component

**File**: `clients/web/src/app/shared/components/list-item/list-item.component.ts` (new)

**Tasks**:
1. Create list item component (56px min height)
2. Add interactive variant with hover states
3. Support content projection for flexible layout

#### 4.4 Toast Component

**File**: `clients/web/src/app/shared/components/toast/toast.component.ts` (new)

**Tasks**:
1. Create toast component with dark background
2. Add icon support
3. Add action button support
4. Create toast service for programmatic display
5. Add entrance/exit animations

#### 4.5 Skeleton Component

**File**: `clients/web/src/app/shared/components/skeleton/skeleton.component.ts` (new)

**Tasks**:
1. Create skeleton loading component
2. Add shimmer animation
3. Support text, avatar, and card skeleton shapes

#### 4.6 Progress Component

**File**: `clients/web/src/app/shared/components/progress/progress.component.ts` (new)

**Tasks**:
1. Create linear progress bar
2. Add member color variant
3. Add animated fill transition

---

### Phase 5: Navigation Updates

#### 5.1 Mobile Tab Bar

**File**: `clients/web/src/app/features/dashboard/components/tab-bar/tab-bar.component.ts` (new)

**Tasks**:
1. Create bottom tab bar for mobile (56px + safe area)
2. Add 5 navigation items with icons
3. Add active state styling
4. Implement safe area insets

#### 5.2 Sidebar Updates

**File**: `clients/web/src/app/features/dashboard/components/dashboard-shell/dashboard-shell.component.ts`

**Tasks**:
1. Update colors to use design tokens
2. Update icon sizes (24px standard)
3. Update active state colors

---

### Phase 6: Animation System

**Files to Modify/Create**:
- `clients/web/src/styles/animations.scss` (new)
- Various component files

**Tasks**:
1. Create keyframe animations (fadeIn, fadeOut, slideUp, scaleIn)
2. Create task completion animation
3. Create checkmark draw animation
4. Add reduced motion media query
5. Update button hover transitions
6. Update card hover transitions
7. Update modal entrance/exit animations
8. Update toast animations

---

### Phase 7: Dashboard & Widget Updates

#### 7.1 Event Card Pattern

**File**: `clients/web/src/app/shared/components/event-card/event-card.component.ts` (new)

**Tasks**:
1. Create event card with colored left border
2. Add time display
3. Add assigned members avatars
4. Add location support

#### 7.2 Task Item Pattern

**File**: `clients/web/src/app/shared/components/task-item/task-item.component.ts` (new)

**Tasks**:
1. Create task item with personal checkbox
2. Add completion animation
3. Add completed strikethrough styling
4. Add due date display

#### 7.3 Empty State Component

**File**: `clients/web/src/app/shared/components/empty-state/empty-state.component.ts` (new)

**Tasks**:
1. Create empty state with icon, title, body
2. Add optional action button
3. Standardize across features

---

### Phase 8: Time-Adaptive Canvas

**Files**:
- `clients/web/src/app/core/services/canvas.service.ts` (new)
- `clients/web/src/styles.scss` (update)

**Tasks**:
1. Create canvas service to manage time-based colors
2. Calculate current period (dawn/morning/afternoon/evening/night)
3. Implement 30-minute transition blending
4. Apply canvas color to body/root element
5. Store user preference (auto/cool/warm/night)

---

### Phase 9: Accessibility Enhancements

**Tasks**:
1. Audit all interactive elements for 44px touch targets
2. Verify color contrast ratios (4.5:1 for text)
3. Add skip link for keyboard navigation
4. Verify focus visible styling throughout
5. Implement reduced motion support
6. Add aria-live regions for toasts
7. Test with screen reader

---

### Phase 10: Polish & Documentation

**Tasks**:
1. Create component documentation/storybook entries
2. Verify all components use design tokens
3. Remove any hardcoded color values
4. Remove unused Tailwind utilities
5. Performance audit (animation performance)
6. Cross-browser testing
7. Responsive testing

---

## File Change Summary

### New Files

| File Path | Purpose |
|-----------|---------|
| `src/styles/design-tokens.scss` | CSS custom properties |
| `src/styles/animations.scss` | Keyframe animations |
| `src/app/shared/components/toggle/` | Toggle switch component |
| `src/app/shared/components/pill/` | Pill/tag component |
| `src/app/shared/components/list-item/` | List item component |
| `src/app/shared/components/toast/` | Toast notifications |
| `src/app/shared/components/skeleton/` | Loading skeletons |
| `src/app/shared/components/progress/` | Progress bars |
| `src/app/shared/components/event-card/` | Calendar event cards |
| `src/app/shared/components/task-item/` | Task list items |
| `src/app/shared/components/empty-state/` | Empty state display |
| `src/app/shared/components/avatar-group/` | Grouped avatars |
| `src/app/features/dashboard/components/tab-bar/` | Mobile navigation |
| `src/app/core/services/canvas.service.ts` | Time-adaptive canvas |
| `src/app/core/services/toast.service.ts` | Toast management |

### Modified Files

| File Path | Changes |
|-----------|---------|
| `tailwind.config.js` | Token updates, new utilities |
| `src/styles.scss` | Design token imports, base styles |
| `src/app/shared/components/button/` | Variant/size updates |
| `src/app/shared/components/card/` | Variant system |
| `src/app/shared/components/avatar/` | Size/color updates |
| `src/app/shared/components/input/` | Size/focus updates |
| `src/app/shared/components/alert/` | Token alignment |
| `src/app/shared/components/spinner/` | Token alignment |
| `src/app/features/dashboard/components/dashboard-shell/` | Nav updates |
| `src/app/features/dashboard/components/dashboard-home/` | Widget updates |

---

## Testing Strategy

### Unit Tests
- Component inputs/outputs work correctly
- Variant/size props render correct classes
- Animations respect reduced motion

### Visual Regression Tests
- Components match design system spec
- Responsive behavior correct
- Dark mode (Night Display) correct

### Accessibility Tests
- axe-core automated testing
- Manual keyboard navigation
- Screen reader testing

### Performance Tests
- Animation frame rate (60fps target)
- No layout thrashing
- Minimal repaints on canvas transitions

---

## Migration Notes

### Breaking Changes

1. **Family Color Names**: `blue` → `sky`, `green` → `emerald`, etc.
2. **Avatar Colors**: Now from profile, not computed from name
3. **Card Variants**: New variant prop may affect existing usage
4. **Button Heights**: Explicit heights may change layout

### Backward Compatibility

- Existing Tailwind classes will continue to work during transition
- Gradual migration with fallbacks
- Feature flag for new styling if needed

---

## Success Criteria

- [ ] All CSS custom properties from design system implemented
- [ ] All existing components updated to use design tokens
- [ ] All new components created and functional
- [ ] Time-adaptive canvas working with smooth transitions
- [ ] Mobile tab bar navigation implemented
- [ ] Animation system complete with reduced motion support
- [ ] WCAG 2.1 AA compliance verified
- [ ] No hardcoded color/spacing values remain
- [ ] Visual design matches specification
- [ ] Performance meets targets (60fps, <100ms interactions)

---

## References

- [Design System Specification](./DESIGN-SYSTEM.md)
- [Architecture Documentation](./ARCHITECTURE.md)
- [Project Overview](./PROJECT-OVERVIEW.md)

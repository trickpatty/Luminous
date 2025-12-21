# ADR-009: Zero-Distraction Design Principle

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Design, Architecture

## Context

Smart displays and tablets often become sources of distraction rather than productivity tools. Children (and adults) are drawn to games, videos, and web browsing when these features are available. This undermines the core purpose of a family command center.

We need to decide whether Luminous should:
- Be a general-purpose smart display with family features
- Be a focused, single-purpose family coordination tool

This decision fundamentally shapes the product's identity and technical architecture.

## Decision Drivers

- **Product differentiation**: What makes Luminous unique in the market
- **Child focus**: Keeping kids on task, not distracted
- **Parent peace of mind**: Knowing the display won't become a distraction
- **Simplicity**: Focused products are easier to build and maintain
- **Feature creep**: Slippery slope from "one game" to "app store"
- **Market positioning**: Aligning with families who actively limit screen time

## Considered Options

### Option 1: General-Purpose Smart Display

Full web browser, app installation, video streaming, voice assistant.

**Pros:**
- Maximum flexibility
- Competes with Amazon Echo Show, Google Nest Hub
- More features = more value (perception)

**Cons:**
- Becomes a distraction device
- Kids gravitate to entertainment
- Parental controls become complex
- Competes with well-funded incumbents
- Dilutes core purpose

### Option 2: Limited Entertainment with Controls

Some entertainment features (games, videos) with parental controls.

**Pros:**
- Rewards for chore completion (screen time)
- Appeals to broader audience
- "Balanced" approach

**Cons:**
- Controls are never perfect
- Kids find workarounds
- Constant negotiation about access
- Maintenance burden for games/videos
- Still a distraction

### Option 3: Zero-Distraction (Focused Purpose)

No web browsing, no games, no videos, no general-purpose voice assistant. Only family coordination features.

**Pros:**
- Clear product identity
- Parents trust it won't distract kids
- Simpler to build and maintain
- Differentiates from smart displays
- Aligns with screen-time-conscious families
- Easier to explain value proposition

**Cons:**
- Some users may want entertainment
- Perceived as "limited"
- Can't use video calling
- Less "cool factor"

## Decision

Luminous will follow a **zero-distraction design principle**. The following capabilities are explicitly excluded:

### Excluded Features

| Feature | Reason |
|---------|--------|
| Web browser | Primary distraction vector |
| App installation | Enables distraction apps |
| Video streaming | Entertainment, not coordination |
| Games (beyond reward animations) | Distraction, not coordination |
| Social media | Distraction, privacy concerns |
| Open-ended voice assistant | Leads to entertainment queries |
| News feeds | Distraction, not family-specific |
| Video calling | Scope creep, better done on other devices |

### Included Features (That Could Be Distracting But Serve Core Purpose)

| Feature | Justification |
|---------|---------------|
| Weather | Glanceable, helps with planning |
| Photo frame mode (idle) | Ambient, not interactive |
| Celebration animations | Brief, positive reinforcement |
| Audio cues for routines | Functional, not entertainment |

## Rationale

1. **Market differentiation**: Every tech company makes smart displays. We differentiate by what we *don't* do. This is our version of Basecamp's "No is easier to do."

2. **Trust with families**: When a parent mounts Luminous on the wall, they know it won't become another screen kids fight over. This trust is our core value proposition.

3. **Competitor alignment**: Skylight explicitly markets "no apps, no web browsing" as a feature, and families respond positively. This validates the approach.

4. **Development focus**: By not building entertainment features, we focus engineering effort on making coordination features excellent.

5. **Child psychology**: The presence of entertainment options, even if controlled, creates negotiation and conflict. Absence removes the issue entirely.

6. **Screen time movement**: Many families are actively reducing screen time. Luminous is an ally, not another source of screen conflict.

## Consequences

### Positive

- Clear product positioning and messaging
- Parents trust the display
- No complex parental control systems
- Simpler architecture and codebase
- Focused development roadmap
- Aligns with screen-time-conscious families

### Negative

- Some users will want entertainment features
- Cannot use gamification with actual games
- May be perceived as "limited" by some
- Video calling must be done on other devices

### Neutral

- Must resist feature requests for entertainment
- Need to clearly communicate product scope
- Rewards system limited to points/achievements, not screen time

## Implementation Notes

### Technical Enforcement

1. **No browser component**: Angular app doesn't include arbitrary URL rendering (only specific integrations like weather APIs)

2. **Kiosk mode**: Electron display app runs in locked-down kiosk mode, no escape to OS

3. **No URL input**: No user-facing way to enter arbitrary URLs

4. **Curated integrations**: External services are curated and purpose-specific

5. **Mobile app focus**: Native iOS and Android apps are similarly focused on coordination only

### Messaging

Product messaging should emphasize:
- "Not a smart display, a family display"
- "No distractions, just coordination"
- "The screen you trust your kids with"
- "What it doesn't do is the point"

### Handling Feature Requests

When users request entertainment features, respond with:
1. Acknowledge the request
2. Explain the design philosophy
3. Suggest alternative devices for that purpose
4. Point to this ADR for detailed reasoning

## Related Decisions

- [ADR-008: Magic Import Requires Approval](./ADR-008-magic-import-approval.md)
- [ADR-002: Angular as Web Framework](./ADR-002-angular-web-framework.md)

## References

- [Skylight Calendar - No Apps, No Web](https://www.skylightcal.com/)
- [Center for Humane Technology](https://www.humanetech.com/)
- [Cal Newport - Digital Minimalism](https://www.calnewport.com/books/digital-minimalism/)

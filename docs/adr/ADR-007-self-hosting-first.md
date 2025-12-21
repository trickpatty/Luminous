# ADR-007: Self-Hosting as Primary Deployment Model

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Architecture, Design

## Context

Luminous is an open-source project that handles sensitive family data. We need to decide on the primary deployment model:

1. Cloud-hosted SaaS (we run the infrastructure)
2. Self-hosted (users run their own infrastructure)
3. Hybrid (both options available)

This decision affects architecture, data ownership, privacy, costs, and the project's sustainability model.

## Decision Drivers

- **Privacy**: Families want control over their personal data
- **Data ownership**: Users should own their data outright
- **Cost sustainability**: Open-source projects need sustainable funding
- **Simplicity for users**: Not all users are technical
- **Project control**: Avoiding infrastructure lock-in
- **Open-source ethos**: Aligning with open-source values

## Considered Options

### Option 1: Cloud-Only SaaS

Luminous runs as a cloud service; users connect their displays and apps.

**Pros:**
- Simple user setup
- Recurring revenue for sustainability
- Easier to push updates
- Centralized support

**Cons:**
- Users don't control their data
- Infrastructure costs
- Privacy concerns
- Vendor lock-in
- Conflicts with open-source ethos

### Option 2: Self-Hosting Only

Users must run their own server; no cloud option provided.

**Pros:**
- Complete user data ownership
- No infrastructure costs for project
- Maximum privacy
- True open-source model

**Cons:**
- High barrier to entry
- Limits adoption to technical users
- Support burden for diverse environments
- Updates require user action

### Option 3: Self-Hosting First, Cloud Optional

Self-hosting is the primary model; cloud is an optional convenience layer.

**Pros:**
- Users can self-host with full features
- Cloud option for less technical users
- Potential revenue from cloud tier
- Maintains open-source credibility
- User choice

**Cons:**
- Must maintain both models
- Cloud still requires infrastructure
- May cannibalize self-hosting

### Option 4: Local-First (No Server Required)

Devices sync peer-to-peer; server is optional even for multi-device.

**Pros:**
- Minimal infrastructure
- True local-first
- Works in isolated networks

**Cons:**
- Peer-to-peer sync is complex
- Devices must be discoverable
- No remote access without server

## Decision

Luminous will use **self-hosting as the primary deployment model**, with the application designed to work fully without any cloud dependency.

### Deployment Tiers

| Tier | Description | Target User |
|------|-------------|-------------|
| **Single Device** | Display + mobile apps sync locally | Non-technical families |
| **Self-Hosted Server** | Docker container on home network | Technical families |
| **Cloud-Hosted** | Managed hosting (future, optional) | Convenience seekers |

## Rationale

1. **Privacy alignment**: Families increasingly care about data privacy. Self-hosting ensures family schedules, chore data, and personal information never leave the home network.

2. **Open-source values**: A self-hostable product aligns with open-source principles. Users aren't dependent on our infrastructure or business continuity.

3. **Cost sustainability**: We don't bear hosting costs for every user. This makes the project more sustainable long-term.

4. **Local-first synergy**: Our local-first architecture (ADR-003) means the server is only needed for multi-device sync, not for basic operation. This reduces the barrier to self-hosting.

5. **Differentiation**: Most competitors are cloud-dependent. Self-hosting is a feature for privacy-conscious families.

6. **Community contribution**: Self-hosters are often the most engaged contributors to open-source projects.

## Consequences

### Positive

- Users own and control their data
- No infrastructure costs for the project
- Strong privacy story
- Attracts privacy-conscious families
- Aligns with open-source community
- Diverse deployment environments strengthen the product

### Negative

- Higher setup barrier for non-technical users
- Must support diverse self-hosted environments
- Less visibility into usage patterns
- Harder to push critical updates
- Potential fragmentation of versions

### Neutral

- Docker becomes a deployment requirement
- Documentation must cover self-hosting thoroughly
- May offer managed hosting later for revenue

## Implementation Notes

### Single-Device Mode (Simplest)

For families with only one display and one or two mobile devices:
- Display runs embedded sync service
- Mobile apps connect to display on local network
- No separate server needed
- Setup: Install display app, scan QR code on mobile

### Self-Hosted Server

For families wanting remote access or multiple displays:
- Docker container with simple configuration
- Runs on Raspberry Pi, NAS, or any Docker host
- Handles sync coordination and push notifications
- Can expose via reverse proxy for remote access

```yaml
# docker-compose.yml
version: '3.8'
services:
  luminous:
    image: luminous/server:latest
    ports:
      - "3000:3000"
    volumes:
      - ./data:/data
    environment:
      - LUMINOUS_SECRET_KEY=${SECRET_KEY}
    restart: unless-stopped
```

### Future Cloud Option

If we offer managed hosting:
- Same Docker image, managed by us
- Optional, paid tier
- Data export always available
- No feature differentiation from self-hosted

### Update Strategy

For self-hosted users:
- Watchtower or Renovate for auto-updates
- In-app update notifications
- Release notes and migration guides
- Semantic versioning for compatibility

## Related Decisions

- [ADR-003: Local-First Data Architecture](./ADR-003-local-first-architecture.md)

## References

- [Self-Hosting Guide (future)](../guides/self-hosting.md)
- [Open Source Sustainability](https://opensource.guide/getting-paid/)
- [Home Assistant - Self-Hosted Model](https://www.home-assistant.io/)

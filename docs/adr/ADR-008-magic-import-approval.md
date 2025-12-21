# ADR-008: Magic Import Requires Approval

> **Status:** Accepted
> **Date:** 2025-12-21
> **Deciders:** Luminous Core Team
> **Categories:** Design, Architecture

## Context

Luminous includes a "Magic Import" feature that uses AI/ML to extract structured data (calendar events, meals, list items) from unstructured inputs (emails, photos, PDFs, text). This dramatically reduces manual data entry.

The key question is: should extracted content be automatically published to the family calendar/lists, or should it require explicit approval?

This decision has significant implications for user trust, data accuracy, and the overall experience.

## Decision Drivers

- **Data accuracy**: AI extraction is not perfect; errors can cause real problems (missed appointments, wrong times)
- **User trust**: Families trust us with important scheduling data
- **Convenience**: The value of magic import is reducing manual work
- **Control**: Users want to feel in control of their family data
- **Spam prevention**: Forwarded emails could contain unwanted content
- **Learning**: System needs feedback to improve extraction

## Considered Options

### Option 1: Auto-Publish Everything

All extracted content is immediately added to calendars/lists.

**Pros:**
- Maximum convenience
- True "magic" experience
- Minimal user effort

**Cons:**
- Errors go straight to calendar
- Spam emails become events
- Users may miss incorrect data
- Loss of control feeling
- Hard to undo bulk imports

### Option 2: Always Require Approval

All imports go to an approval queue; nothing is published without explicit action.

**Pros:**
- User always in control
- Errors caught before publishing
- Spam filtered naturally
- Provides feedback for model improvement
- Builds trust through transparency

**Cons:**
- Additional step for every import
- May feel like extra work
- Queue can pile up if ignored
- Reduces "magic" feeling

### Option 3: Confidence-Based Auto-Publish

High-confidence extractions auto-publish; low-confidence goes to queue.

**Pros:**
- Best of both approaches
- Reduces approval burden for clear cases
- Still catches uncertain extractions

**Cons:**
- Confidence thresholds are arbitrary
- Users may not understand why some auto-publish
- Inconsistent experience
- False confidence can cause errors

### Option 4: Per-Source Trust Levels

Users configure which sources auto-publish (e.g., school emails = auto, unknown = review).

**Pros:**
- User-controlled automation
- Reduces burden for trusted sources
- Clear mental model

**Cons:**
- Setup complexity
- Users may not configure properly
- Trusted sources can still have errors
- Attackers could spoof trusted sources

## Decision

We will require **explicit approval for all Magic Import extractions** before publishing to calendars, lists, or other family data.

No AI-extracted content will appear on the family calendar or lists until a household admin or adult member approves it.

## Rationale

1. **Trust is paramount**: Families are trusting us with their most important coordination data. A single missed soccer game due to an import error could undermine all trust in the system. The approval step makes users partners in the process.

2. **AI accuracy**: Current extraction technology, while impressive, is not perfect. Time zone issues, date format ambiguities, and OCR errors are common. Human review catches these.

3. **Spam and security**: Email forwarding opens a potential vector for unwanted content. The approval queue acts as a filter.

4. **Feedback loop**: Approvals and rejections provide valuable training signal. Users correcting extractions helps the system improve over time.

5. **Mental model**: Users understand "I forwarded an email, now I approve what was extracted." This is clearer than "sometimes it appears, sometimes it doesn't."

6. **Industry precedent**: Competitor products (Hearth Helper) use approval workflows and users accept them.

## Consequences

### Positive

- Users feel in control of their data
- Errors are caught before impacting family
- Natural spam filtering
- Training data for model improvement
- Clear, consistent user experience
- Builds trust in the AI features

### Negative

- Extra step for every import
- Approval queue requires attention
- May feel less "magical"
- Users must check queue regularly
- Mobile notifications needed for pending imports

### Neutral

- Need to design efficient approval UI (batch approve, quick edits)
- Need to handle stale items in queue
- Should track approval/rejection rates for quality monitoring

## Implementation Notes

### Approval Queue

- Visible on display and mobile app
- Push notification for new pending items
- Batch approval with individual editing
- Auto-expire old unapproved items (30 days with warning)
- Show extraction confidence to help users prioritize review

### Approval UI Features

```
┌─────────────────────────────────────────────────────┐
│ 📬 Magic Import Queue (3 pending)                   │
├─────────────────────────────────────────────────────┤
│                                                     │
│ ┌─────────────────────────────────────────────────┐ │
│ │ 📅 Soccer Practice                              │ │
│ │ Extracted from: school-email.pdf                │ │
│ │ When: Tuesdays & Thursdays, 4:00 PM             │ │
│ │ Where: Main Field                               │ │
│ │ Assign to: [Emma ▼]                             │ │
│ │                                                 │ │
│ │ [✓ Approve]  [Edit]  [✗ Reject]                │ │
│ └─────────────────────────────────────────────────┘ │
│                                                     │
│ [Approve All]  [Review Later]                       │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Azure Implementation

The Magic Import pipeline runs as Azure Functions:

1. **Import Trigger Function**: Receives emails/uploads via Azure Blob Storage
2. **Extraction Function**: Uses Azure AI Document Intelligence for OCR and extraction
3. **Queue Storage**: Pending imports stored in Cosmos DB with `status: 'pending'`
4. **Approval API**: .NET API endpoint for approve/reject actions
5. **SignalR Notification**: Real-time updates when new imports arrive

### Future Considerations

We may revisit this decision if:
- Extraction accuracy reaches very high levels (>99%)
- Users strongly request auto-publish option
- Trusted source configuration proves reliable

Any such change would be opt-in and default to approval-required.

## Related Decisions

- [ADR-009: Zero-Distraction Design Principle](./ADR-009-zero-distraction-principle.md)
- [ADR-005: CosmosDB as Primary Data Store](./ADR-005-cosmosdb-data-store.md)

## References

- [Hearth Display Helper Feature](https://www.hearthapp.com/)
- [Design for Trust in AI Systems](https://www.nngroup.com/articles/ai-trust/)
- [Azure AI Document Intelligence](https://learn.microsoft.com/azure/ai-services/document-intelligence/)

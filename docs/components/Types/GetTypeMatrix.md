# Get Type Matrix

Builds the full attacking/defending type-effectiveness matrix in one call — no inputs, no manual
wiring of 18 separate lookups.

## Inputs

None — internally lists and fetches every type.

## Outputs

| Name | Type | Description |
|---|---|---|
| Attacking Type | text (list) | Attacking type name, one entry per matrix cell |
| Defending Type | text (list) | Defending type name, one entry per matrix cell |
| Multiplier | number (list) | Damage multiplier (0, 0.5, 1, or 2), parallel to Attacking/Defending Type |
| Status | text | Success or error status |
| Response | text | Raw JSON response of the type list call |

## API endpoint

`GET /api/v2/type/` (list), then `GET /api/v2/type/{id}/` for each of the 18 types

## Notes

- Outputs exactly 324 entries (18 x 18) across the three parallel lists.
- Makes 19 HTTP calls total — slower than a single lookup, but a single placed component instead
  of wiring 18 [Get Type](GetType.md) calls by hand.

## Related

- [Get Type](GetType.md) — the granular single-type primitive this component wraps

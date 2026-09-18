# Dual Type Matchup

*(v2.0.0)* Combines two types' defenses into a dual-type Pokemon's true damage multipliers
(4x/2x/1x/0.5x/0.25x/0x) against every attacking type — the real defensive picture for a two-type
Pokemon, not just one type's own relations. Type 2 is optional — leave it empty for a single-type
Pokemon.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Type 1 | text | yes | "" | First (or only) elemental type name |
| Type 2 | text | no | "" | Second elemental type name |

## Outputs

| Name | Type | Description |
|---|---|---|
| 4x Damage From | text (list) | Attacking types that deal 4x damage |
| 2x Damage From | text (list) | Attacking types that deal 2x damage |
| 1x Damage From | text (list) | Attacking types that deal neutral (1x) damage |
| 0.5x Damage From | text (list) | Attacking types that deal 0.5x damage |
| 0.25x Damage From | text (list) | Attacking types that deal 0.25x damage |
| 0x Damage From | text (list) | Attacking types this combination is immune to |
| Status | text | Success or error status |
| Response | text | Raw JSON responses for both types, as a JSON array |

## API endpoint

`GET /api/v2/type/{id}/` (called once or twice — same endpoint as [Get Type](GetType.md))

## Related

- [Get Type](GetType.md) — single type's full to/from relation set
- [Type Matchup](TypeMatchup.md) — single type's outgoing relations only

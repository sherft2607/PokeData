# Type Matchup

*(v2.0.0)* Looks up which types an elemental type deals double, half, or no damage to — a focused
counterpart to [Get Type](GetType.md) for "what should I use against X" workflows.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Attacking Type | text | yes | "" | Elemental type name, e.g. `"fire"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| 2x Damage To | text (list) | Types this type deals double damage to |
| 0.5x Damage To | text (list) | Types this type deals half damage to |
| 0x Damage To | text (list) | Types this type deals no damage to |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/type/{id}/` (same endpoint as [Get Type](GetType.md))

## Related

- [Get Type](GetType.md) — the full to/from relation set plus associated Pokemon and type color

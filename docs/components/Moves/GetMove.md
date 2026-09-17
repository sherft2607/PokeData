# Get Move

Looks up a move's power, accuracy, PP, damage class, type, and effect text.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Move Name Or ID | text | yes | "" | Move name or numeric ID, e.g. `"thunderbolt"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Power | integer | Base power (may be empty for status moves) |
| Accuracy | integer | Accuracy percentage (may be empty for moves that always hit) |
| PP | integer | Power points |
| Damage Class | text | physical, special, or status |
| Type | text | Elemental type of the move |
| Effect Text | text | Short English effect description |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/move/{id}/`

## Related

- [Get Ability](GetAbility.md) — same lookup pattern, for abilities

# Get Type

Looks up an elemental type's damage relations to and from every other type, plus every Pokemon
that has this type.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Type Name | text | yes | "" | Elemental type name, e.g. `"fire"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Double Damage To | text (list) | Types this type deals double damage to |
| Double Damage From | text (list) | Types this type takes double damage from |
| Half Damage To | text (list) | Types this type deals half damage to |
| Half Damage From | text (list) | Types this type takes half damage from |
| No Damage To | text (list) | Types this type deals no damage to |
| No Damage From | text (list) | Types this type takes no damage from |
| Status | text | Success or error status |
| Response | text | Raw JSON response |
| Associated Pokemon | text (list) | Every Pokemon that has this type |
| Color | colour | Canonical display color for this elemental type (v2.0.0, not from the API) |

## API endpoint

`GET /api/v2/type/{id}/`

## Notes

- The [Type Name preset](../Presets/PokemonTypePreset.md) wires directly into this component's
  `Type Name` input.
- `Associated Pokemon` can be a long list for common types (e.g. `water`, `normal`) — a few hundred
  entries is normal.

## Related

- [Get Type Matrix](GetTypeMatrix.md) — the full 18x18 matrix in one call, built from repeated
  calls to this same endpoint
- [Type Matchup](TypeMatchup.md) — just the outgoing (attacking) relations, split into 3 lists
- [Type Name preset](../Presets/PokemonTypePreset.md)

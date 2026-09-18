# Type Palette

*(v2.0.0)* Returns the canonical display colors for one or two elemental types, plus a blended
color for dual-type swatches. Pure lookup — not a PokeAPI endpoint.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Type 1 | text | yes | "" | First (or only) elemental type name |
| Type 2 | text | no | "" | Second elemental type name — leave empty for a single-type Pokemon |

## Outputs

| Name | Type | Description |
|---|---|---|
| Color 1 | colour | Canonical color for Type 1 |
| Color 2 | colour | Canonical color for Type 2 (same as Color 1 if Type 2 is empty) |
| Blend Color | colour | Average-channel blend of Color 1 and Color 2 |
| Status | text | Success or error status |

## Related

- [Get Type](../Types/GetType.md) — also exposes a single-type `Color` output
- [Canvas Sprite Card](CanvasSpriteCard.md)

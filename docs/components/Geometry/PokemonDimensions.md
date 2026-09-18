# Pokemon Dimensions

*(v2.0.0)* Converts a Pokemon's raw PokeAPI height/weight (decimetres/hectograms — e.g. Get
Pokemon's Height/Weight outputs) into metric and imperial units, plus a reference bounding Box
sized to the converted height. Pure conversion — not a PokeAPI endpoint.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Height | number | no | 0 | Height in decimetres, e.g. Get Pokemon's Height output |
| Weight | number | no | 0 | Weight in hectograms, e.g. Get Pokemon's Weight output |
| Plane | plane | no | WorldXY | Base plane for the reference Box |

## Outputs

| Name | Type | Description |
|---|---|---|
| Height (m) | number | Height in meters |
| Height (ft) | number | Height in feet |
| Weight (kg) | number | Weight in kilograms |
| Weight (lb) | number | Weight in pounds |
| Reference Box | box | A box sized to the converted height, footprint proportional to it |
| Status | text | Success or error status |

## Related

- [Get Pokemon](../Pokemon/GetPokemon.md) — the usual source of Height/Weight

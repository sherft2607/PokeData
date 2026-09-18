# Stat Radar

*(v2.0.0)* Turns a Pokemon's 6 base stat values (e.g. [Get Pokemon](../Pokemon/GetPokemon.md)'s
`Stat Values` output) into a normalized 2D radar/spider chart polygon. Pure canvas geometry — not
a PokeAPI endpoint.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Stat Values | integer (list) | yes | — | Base stat values, e.g. Get Pokemon's `Stat Values` output |
| Radius | number | no | 10.0 | Radius at maximum stat value |
| Max Stat | number | no | 255.0 | Stat value that maps to the full radius (canonical base-stat ceiling) |
| Center | point | no | origin | Center point of the radar chart |

## Outputs

| Name | Type | Description |
|---|---|---|
| Radar Points | point (list) | One point per stat value, evenly spaced around Center |
| Radar Polyline | curve | Closed polyline through the radar points |
| Status | text | Success or error status |

## Notes

- Needs at least 3 stat values to form a polygon.
- Values are clamped to `[0, Max Stat]` before scaling, so an outlier never produces a point
  outside `Radius`.
- Color-by-type or size-by-stat mapping is deliberately left to native Grasshopper components
  (Gradient, Remap Numbers) — this component only produces the geometry.

## Related

- [Get Pokemon](../Pokemon/GetPokemon.md) — the usual source of `Stat Values`
- [Get Type](../Types/GetType.md) — pairs well for coloring the radar by type (`Color` output)

# Stat Mesh 3D

*(v2.0.0)* Extrudes a fixed-radius regular polygon into a 3D "radar skyline" Mesh, where each
vertex's height is driven by one base stat value — a 3D counterpart to
[Stat Radar](StatRadar.md)'s flat polyline. Pure canvas geometry — not a PokeAPI endpoint.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Stat Values | integer (list) | yes | — | Base stat values, e.g. Get Pokemon's `Stat Values` output |
| Height Factor | number | no | 1.0 | Scales how tall the extrusion gets at the maximum stat value |
| Radius | number | no | 10.0 | Radius of the base/top ring |
| Max Stat | number | no | 255.0 | Stat value that maps to full extrusion height |
| Center | point | no | origin | Center point of the mesh |

## Outputs

| Name | Type | Description |
|---|---|---|
| Mesh | mesh | The extruded 3D radar mesh |
| Status | text | Success or error status |

## Notes

- Needs at least 3 stat values.
- Ring positions are fixed-radius (unlike Stat Radar, where radius varies per stat) — only the
  extrusion height encodes the stat value.

## Related

- [Stat Radar](StatRadar.md) — the flat 2D version

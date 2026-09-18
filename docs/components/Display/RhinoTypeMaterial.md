# Rhino Type Material

*(v3.0.0)* Builds a simple diffuse `Rhino.Render.RenderMaterial` from one or two canonical type
colors ([Type Palette](TypePalette.md)'s same lookup/blend), optionally adding it to the active
document's render material table. Pure lookup plus a native Rhino material wrapper — not a
PokeAPI endpoint.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Type 1 | text | yes | "" | First (or only) elemental type name |
| Type 2 | text | no | "" | Second elemental type name — leave empty for a single-type Pokemon |
| Add To Document | boolean | no | false | Set true to add the material to the active Rhino document's render material table |

## Outputs

| Name | Type | Description |
|---|---|---|
| Material | generic | The built `Rhino.Render.RenderMaterial` |
| Color | colour | The diffuse color used (Color 1 alone, or the blend of Color 1/Color 2) |
| Status | text | Success or error status |

## Notes

- Built via `Rhino.DocObjects.Material` → its `.RenderMaterial` property, rather than the more
  version-sensitive `Rhino.Render.RenderContentType` APIs — keeps this stable across the Rhino
  7/8 RhinoCommon versions this plugin multi-targets.
- Scoped to a simple diffuse material — no artwork color clustering (flagged as a future idea).

## Related

- [Type Palette](TypePalette.md) — the same colors as flat `colour` outputs instead of a material

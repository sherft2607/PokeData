# Canvas Sprite Card

*(v2.0.0)* Builds a flat rectangular card Mesh at a point, sized to hold a sprite + stat text,
plus a combined label string and a display Plane for native GH components (Picture Frame, Text Tag
3D) to consume — no texture is baked onto the mesh here, since that needs a display conduit
rather than mesh geometry. Pure canvas layout — not a PokeAPI endpoint.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Name | text | no | "" | Pokemon (or card title) name |
| Stats | text (list) | no | — | Stat display lines, e.g. `"hp: 35"` per item |
| Sprite Bitmap | generic | no | — | Decoded sprite Bitmap, passed through for a Picture Frame |
| Point | point | no | origin | Card placement point (lower-left corner) |
| Width | number | no | 10 | Card width |
| Height | number | no | 14 | Card height |

## Outputs

| Name | Type | Description |
|---|---|---|
| Card Mesh | mesh | Flat rectangular card backing |
| Label | text | Name and stat lines joined into one label string |
| Card Plane | plane | Plane at Point, for wiring into Picture Frame / Text Tag 3D |
| Sprite Bitmap | generic | Passthrough of the input Bitmap |
| Status | text | Success or error status |

## Related

- [Sprite Downloader](../Pokemon/SpriteDownloader.md) — the usual source of Sprite Bitmap
- [Type Palette](TypePalette.md)

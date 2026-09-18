# Sprite To Voxel

*(v3.0.0)* Voxelizes an in-memory sprite Bitmap (e.g. [Sprite Downloader](../Pokemon/SpriteDownloader.md)'s
output) into a box mesh — one box per opaque pixel, with per-vertex RGBA vertex colors — plus a
separate luminance heightfield relief mesh. Pure canvas geometry — not a PokeAPI endpoint.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Sprite Bitmap | generic | yes | — | Decoded sprite Bitmap |
| Voxel Size | number | no | 1.0 | Edge length of each voxel box |
| Alpha Threshold | integer | no | 10 | Pixels at or below this alpha are treated as background, 0-255 |
| Heightfield Scale | number | no | 5.0 | Scales the luminance heightfield's Z displacement |
| Max Resolution | integer | no | 64 | Downsamples so neither dimension exceeds this many pixels |

## Outputs

| Name | Type | Description |
|---|---|---|
| Voxel Mesh | mesh | One box per opaque pixel, with per-vertex RGBA vertex colors |
| Heightfield Mesh | mesh | A grid mesh displaced by per-pixel luminance |
| Voxel Count | integer | Number of opaque pixels voxelized |
| Status | text | Success or error status |

## Notes

- Official artwork sprites can be 475x475+ pixels; **Max Resolution** downsamples first so the
  mesh stays a reasonable size — raise it for more detail at the cost of a much larger mesh.
- Luminance uses Rec. 709 coefficients (`0.2126*R + 0.7152*G + 0.0722*B`).

## Related

- [Sprite Downloader](../Pokemon/SpriteDownloader.md) — the usual source of Sprite Bitmap
- [Batch Downloader](../Pokemon/BatchDownloader.md)

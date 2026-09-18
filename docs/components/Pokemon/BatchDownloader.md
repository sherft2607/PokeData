# Batch Downloader

*(v2.0.0)* Downloads a list of image URLs in parallel (`Task.WhenAll` over the same in-memory
`GetImageBytesAsync` helper Sprite Downloader uses) and decodes each to a Bitmap. Gated by
`Trigger` so a list of URLs on the canvas doesn't refetch on every solve. Not a PokeAPI endpoint —
sprites/artwork are hosted on `raw.githubusercontent.com`.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| URLs | text (list) | yes | — | Image URLs to download, e.g. a list of sprite URLs |
| Trigger | boolean | no | false | Set true to download every URL |

## Outputs

| Name | Type | Description |
|---|---|---|
| Bitmaps | generic (list) | Decoded Bitmaps, parallel to URLs (null where a download/decode failed) |
| Status | text | Overall success/failure summary, e.g. `"Partial: 2 succeeded, 1 failed"` |

## Related

- [Sprite Downloader](SpriteDownloader.md) — the single-URL version this reuses

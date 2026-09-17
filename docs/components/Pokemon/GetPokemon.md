# Get Pokemon

Looks up a Pokemon by name or ID and returns its types, size, stats, sprite URLs, and cry URLs.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Pokemon Name Or ID | text | yes | "" | Pokemon name or numeric ID, e.g. `"pikachu"` or `25` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Types | text (list) | Elemental types |
| Height | number | Height in decimetres |
| Weight | number | Weight in hectograms |
| Base Experience | integer | Base experience yield |
| Stat Names | text (list) | hp, attack, defense, special-attack, special-defense, speed |
| Stat Values | integer (list) | Base stat values, parallel to Stat Names |
| Sprite Front URL | text | Front-facing default sprite image URL |
| Sprite Artwork URL | text | Official artwork image URL |
| Status | text | Success or error status |
| Response | text | Raw JSON response |
| Legacy Cry URL | text | Legacy (pre-Gen IX) cry audio URL |
| Latest Cry URL | text | Latest cry audio URL |

## API endpoint

`GET /api/v2/pokemon/{id}/`

## Example response

```json
{
  "name": "pikachu",
  "height": 4,
  "weight": 60,
  "base_experience": 112,
  "types": [{ "slot": 1, "type": { "name": "electric" } }],
  "stats": [{ "base_stat": 35, "stat": { "name": "hp" } }],
  "sprites": {
    "front_default": "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/25.png",
    "other": { "official-artwork": { "front_default": "https://raw.githubusercontent.com/.../25.png" } }
  },
  "cries": {
    "latest": "https://raw.githubusercontent.com/PokeAPI/cries/main/cries/pokemon/latest/25.ogg",
    "legacy": "https://raw.githubusercontent.com/PokeAPI/cries/main/cries/pokemon/legacy/25.ogg"
  }
}
```

## Notes

- Accepts either the Pokemon's name or its numeric ID in the same input.
- Wire `Sprite Front URL` or `Sprite Artwork URL` into [Sprite Downloader](SpriteDownloader.md) to
  get an actual image on the canvas.

## Related

- [Get Pokemon Batch](GetPokemonBatch.md) — same fields, for a list of Pokemon at once
- [Sprite Downloader](SpriteDownloader.md) — turns the sprite URL outputs into a Bitmap

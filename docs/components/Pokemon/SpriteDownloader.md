# Sprite Downloader

Downloads an image URL and decodes it to a Bitmap for use with image samplers.

Not a PokeAPI endpoint — sprites are hosted on `raw.githubusercontent.com`, a different host than
`pokeapi.co`. This component just fetches whatever URL it's given and decodes it.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Sprite URL | text | yes | "" | Image URL, e.g. Get Pokemon's Sprite Front URL or Sprite Artwork URL output |

## Outputs

| Name | Type | Description |
|---|---|---|
| Bitmap | generic | Decoded image, as a System.Drawing.Bitmap |
| Status | text | Success or error status |

## Notes

- Works with any publicly reachable image URL, not just PokeAPI sprites.
- The `Bitmap` output is a plain .NET object — wire it into any component that accepts a generic
  image/Bitmap input.

## Related

- [Get Pokemon](GetPokemon.md) — its Sprite Front URL / Sprite Artwork URL outputs feed this
  component directly

# Pokemon Cry

*(v2.0.0)* Looks up a Pokemon's cry audio URLs and, when triggered, streams the audio bytes
in-memory and plays it through the OS's default player.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Pokemon Name Or ID | text | yes | "" | Pokemon name or numeric ID, e.g. `"pikachu"` or `25` |
| Play | boolean | no | false | Set true to download and play the Latest Cry audio |

## Outputs

| Name | Type | Description |
|---|---|---|
| Legacy Cry URL | text | Legacy (pre-Gen IX) cry audio URL |
| Latest Cry URL | text | Latest cry audio URL |
| Status | text | Success or error status |

## API endpoint

`GET /api/v2/pokemon/{id}/` (same endpoint as [Get Pokemon](GetPokemon.md); cry URLs come from
its `cries` field)

## Notes

- Cry files themselves are hosted on `raw.githubusercontent.com`, not `pokeapi.co`.
- Audio is downloaded fully in-memory; setting `Play` writes a transient `.ogg` file to the OS
  temp folder only as a handoff to the system's default audio player (no in-process `.ogg`
  decoder is shipped with this plugin), then hands the file off — this plugin does not read it
  back or hold a lock on it afterward.

## Related

- [Get Pokemon](GetPokemon.md) — also exposes `Legacy Cry URL` / `Latest Cry URL` without playback

# Pokemon lookup

_Demo file pending — built from the plugin spec, not a saved canvas._

**Goal:** Look up a single Pokemon by name and read back its types, size, stats, and sprite URLs.

**Components:**
- [Get Pokemon](../components/Pokemon/GetPokemon.md)

**Inputs to set:**
- `Pokemon Name Or ID` — `"pikachu"`

**Expected result:**
- `Status` = `OK`
- `Types` = `electric`
- `Height` = `4`, `Weight` = `60`
- `Stat Names` / `Stat Values` — 6 parallel entries
- `Sprite Front URL` / `Sprite Artwork URL` — non-empty image URLs
- `Legacy Cry URL` / `Latest Cry URL` — non-empty `.ogg` URLs

See [demos/README.md](https://github.com/sherft2607/PokeData/blob/main/demos/README.md) for the
full manual build spec.

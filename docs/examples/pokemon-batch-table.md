# Batch Pokedex table

_Demo file pending — built from the plugin spec, not a saved canvas._

**Goal:** Feed a list of Pokemon names in and get back a parallel tree of their fields.

**Components:**
- [Get Pokemon Batch](../components/Pokemon/GetPokemonBatch.md)

**Inputs to set:**
- `Pokemon Names Or IDs` — `pikachu`, `bulbasaur`, `charmander`, `squirtle`, `eevee`

**Expected result:**
- `Status` tree has 5 branches, each `OK`
- All other outputs have 5 branches, one per input Pokemon, in the same order as the input list

See [demos/README.md](https://github.com/sherft2607/PokeData/blob/main/demos/README.md) for the
full manual build spec.

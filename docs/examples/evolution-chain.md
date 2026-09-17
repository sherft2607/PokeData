# Evolution chain graph

_Demo file pending — built from the plugin spec, not a saved canvas._

**Goal:** Resolve a species to its full evolution chain, flattened into parent/child/trigger
edges.

**Components:**
- [Get Evolution Chain](../components/Evolution/GetEvolutionChain.md)

**Inputs to set:**
- `Species Name Or ID` — `"bulbasaur"`

**Expected result:**
- `Status` = `OK`
- `Parent Species` = `[bulbasaur, ivysaur]`
- `Child Species` = `[ivysaur, venusaur]`
- `Trigger` = `[level-up, level-up]`

(Optional: try `eevee` for a branching chain — 8 edges, all with `Parent Species` = `eevee`.)

See [demos/README.md](https://github.com/sherft2607/PokeData/blob/main/demos/README.md) for the
full manual build spec.

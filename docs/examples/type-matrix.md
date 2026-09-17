# Type effectiveness matrix

_Demo file pending — built from the plugin spec, not a saved canvas._

**Goal:** Build the full 18x18 type-effectiveness matrix in one component.

**Components:**
- [Get Type Matrix](../components/Types/GetTypeMatrix.md)

**Inputs to set:** none — no inputs at all.

**Expected result:**
- `Attacking Type`, `Defending Type`, `Multiplier` each have exactly 324 items
- Spot-check: `AT` = `fire`, `DT` = `grass` → `M` = `2`; `AT` = `fire`, `DT` = `water` → `M` = `0.5`

See [demos/README.md](https://github.com/sherft2607/PokeData/blob/main/demos/README.md) for the
full manual build spec.

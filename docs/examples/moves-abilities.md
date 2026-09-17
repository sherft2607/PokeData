# Move and ability lookup

_Demo file pending — built from the plugin spec, not a saved canvas._

**Goal:** Look up a move and an ability's mechanical fields side by side, for team-planning
tools.

**Components:**
- [Get Move](../components/Moves/GetMove.md)
- [Get Ability](../components/Moves/GetAbility.md)

**Inputs to set:**
- `Move Name Or ID` — `"thunderbolt"`
- `Ability Name Or ID` — `"static"`

**Expected result:**
- Get Move: `Status` = `OK`, `Power` = `90`, `Accuracy` = `100`, `PP` = `15`,
  `Damage Class` = `special`, `Type` = `electric`
- Get Ability: `Status` = `OK`, `Generation` = `generation-iii`

See [demos/README.md](https://github.com/sherft2607/PokeData/blob/main/demos/README.md) for the
full manual build spec.

# PokeData demo canvases — manual build spec

No Grasshopper MCP bridge was wired in this session (no RhinoMCP / cordyceps / SandMartin tool
available), so Job A/B of `pr-rhino-test` were skipped automatically rather than hard-failing the
pipeline. These are the manual build instructions every no-bridge PleaseREST run falls back to.
Build these five canvases by hand in Grasshopper, save each as `demos/[name].gh`, and confirm them
at Gate 6 — no token panel needed anywhere, since PokeAPI is fully public and unauthenticated.

---

## 1. pokemon-lookup.gh

**Goal:** Look up a single Pokemon by name and read back its types, size, stats, and sprite URLs.

**Components:**
- `Get Pokemon` (Poke) — Pokemon tab

**Wiring:** none — single standalone component, all outputs to panels.

**Inputs to set:**
- `Pokemon Name Or ID` (P) — Panel/text: `"pikachu"`

**Expected result:**
- `Status` (S) = `OK`
- `Types` (TY) = `electric`
- `Height` (H) = `4`, `Weight` (W) = `60`
- `Stat Names` / `Stat Values` — 6 parallel entries (hp, attack, defense, special-attack,
  special-defense, speed)
- `Sprite Front URL` / `Sprite Artwork URL` — non-empty `https://raw.githubusercontent.com/...` URLs

---

## 2. pokemon-batch-table.gh

**Goal:** Feed a list of Pokemon names in and get back a parallel tree of their fields — the
tabular-benchmark workflow.

**Components:**
- `Get Pokemon Batch` (PokeB) — Pokemon tab

**Wiring:** none — single component, list input from a panel, tree outputs to panels.

**Inputs to set:**
- `Pokemon Names Or IDs` (P) — Panel/text, one item per line: `pikachu`, `bulbasaur`, `charmander`,
  `squirtle`, `eevee`

**Expected result:**
- `Status` (S) tree has 5 branches, each `OK`
- `Types`, `Height`, `Weight`, `Stat Names`/`Stat Values`, both sprite URL outputs each have 5
  branches, one per input Pokemon, in the same order as the input list

---

## 3. type-matrix.gh

**Goal:** Build the full 18x18 type-effectiveness matrix in one component — no manual wiring of 18
lookups.

**Components:**
- `Get Type Matrix` (TypeMx) — Types tab

**Wiring:** none — no inputs at all; outputs straight to panels.

**Expected result:**
- `Status` (S) = `OK`
- `Attacking Type` (AT), `Defending Type` (DT), `Multiplier` (M) each have exactly 324 items
  (18 x 18)
- Spot-check: where `AT` = `fire` and `DT` = `grass`, the parallel `M` entry = `2` (fire is
  super-effective against grass); where `AT` = `fire` and `DT` = `water`, `M` = `0.5`

---

## 4. evolution-chain.gh

**Goal:** Resolve a species to its full evolution chain, flattened into parent/child/trigger edges,
without manually chaining a species lookup into a separate evolution-chain lookup.

**Components:**
- `Get Evolution Chain` (EvoChain) — Evolution tab

**Wiring:** none — single component.

**Inputs to set:**
- `Species Name Or ID` (SP) — Panel/text: `"bulbasaur"`

**Expected result:**
- `Status` (S) = `OK`
- `Parent Species` (P) = `[bulbasaur, ivysaur]`
- `Child Species` (C) = `[ivysaur, venusaur]`
- `Trigger` (TR) = `[level-up, level-up]`

(Optional second check with a branching species like `eevee` — expect 8 edges, one per eeveelution,
all with `Parent Species` = `eevee`.)

---

## 5. moves-abilities.gh

**Goal:** Look up a move and an ability's mechanical fields side by side, for team-planning tools.

**Components:**
- `Get Move` (Move) — Moves tab
- `Get Ability` (Ability) — Moves tab

**Wiring:** none — two independent components, both outputs to panels.

**Inputs to set:**
- `Move Name Or ID` (M) — Panel/text: `"thunderbolt"`
- `Ability Name Or ID` (A) — Panel/text: `"static"`

**Expected result:**
- `Get Move`: `Status` = `OK`, `Power` = `90`, `Accuracy` = `100`, `PP` = `15`,
  `Damage Class` = `special`, `Type` = `electric`, `Effect Text` non-empty
- `Get Ability`: `Status` = `OK`, `Generation` = `generation-iii`, `Effect Text` non-empty

---

## Batch 1 additions (extend round 1)

- **Get Pokemon** gained two appended outputs: `Legacy Cry URL` (LC) / `Latest Cry URL` (LT) —
  add these to the pokemon-lookup.gh check: for `pikachu`, both should be non-empty
  `https://raw.githubusercontent.com/...` `.ogg` URLs.
- **Get Type** gained an appended output: `Associated Pokemon` (AP) — for `fire`, this list
  should be non-empty and include `charmander`.
- **Get Pokemon Species** (new, Pokemon tab) — `Species Name Or ID` = `"bulbasaur"` should give
  `Color` = `green`, `Shape` = `quadruped`, `Is Legendary` = `false`, `Egg Groups` containing
  `monster` and `grass`.
- **Sprite Downloader** (new, Pokemon tab) — wire Get Pokemon's `Sprite Front URL` output into
  its `Sprite URL` input; `Status` should read `OK` and the `Bitmap` output should be a decoded
  image (wire it into a native GH Bitmap-consuming component, e.g. a custom preview, to confirm).

## Also worth placing once (not a numbered demo)

- `Type Name` preset (`PokemonTypePreset`, Presets tab) wired into `Get Type`'s `Type Name` input —
  confirms the value-list output name/nickname (`TN`) lines up with the consumer input, per the spec's
  I/O line-up check.

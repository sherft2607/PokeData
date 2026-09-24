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

## v2.0.0 additions

- **Pokemon Cry** (new, Pokemon tab) — `Pokemon Name Or ID` = `"pikachu"`, `Play` = `false`:
  `Status` = `OK`, `Legacy Cry URL` / `Latest Cry URL` both non-empty `.ogg` URLs. Then set
  `Play` = `true` and confirm the cry audio plays via the OS default player (a transient temp
  `.ogg` file is written to `%TEMP%` only at this point).
- **Type Matchup** (new, Types tab) — `Attacking Type` = `"fire"`: `Status` = `OK`,
  `2x Damage To` contains `grass`, `ice`, `bug`, `steel`; `0.5x Damage To` contains `fire`,
  `water`, `rock`, `dragon`; `0x Damage To` is empty.
- **Get Move** gained two appended outputs: `Priority` (PR) / `Description` (D) — for
  `"thunderbolt"`, `Priority` = `0`; `Description` should be a non-empty list with one entry per
  language.
- **Get Ability** gained an appended output: `Short Effect` (SE) — for `"static"`, a non-empty
  list with one entry per language.
- **Get Type** gained an appended output: `Color` (CO) — for `"fire"`, a swatch/panel should show
  an orange-red color (canonical type color, not from the API).
- **Stat Radar** (new, Visualization tab) — wire `Get Pokemon`'s `Stat Values` output (for
  `"pikachu"`) into `Stat Values`; leave `Radius`/`Max Stat`/`Center` at defaults: `Status` = `OK`,
  `Radar Points` has 6 points, `Radar Polyline` is a closed 6-sided polygon visible in the Rhino
  viewport.

## v2.0.0 batch 2 additions (10-component round, same unreleased version)

- **Dual Type Matchup** (Types tab) — `Type 1` = `"fire"`, `Type 2` = `"flying"`: `Status` = `OK`,
  `4x Damage From` contains `rock`, `1x Damage From` contains `ice`, `0x Damage From` contains
  `ground`. Leave `Type 2` empty and confirm it matches `Type Matchup`'s fire-only results.
- **Get Generation** (new, Data tab) — `Generation` = `1`: `Status` = `OK`, `Species IDs` has 151
  entries starting `1`, `Species Names` starts with `bulbasaur`, `Region` = `kanto`.
- **Pokemon Filter** (new, Data tab) — wire `Get Generation`'s `Species Names` into `Names` and a
  matching-length list of numbers into `Values` (e.g. species IDs cast to number); `Filter Rule` =
  `"LessThan"`, `Max` = `10`: `Filtered Names` should be the species with IDs 1-9, `Match Indices`
  parallel and 0-based.
- **Get Item** (new, Items tab) — `Item Name Or ID` = `"poke-ball"`: `Status` = `OK`, `Category` =
  `standard-balls`, `Effect Text` non-empty, `Sprite URL` a non-empty
  `raw.githubusercontent.com` URL.
- **Get Nature** (new, Stats tab) — `Nature Name Or ID` = `"adamant"`: `Increased Stat` = `attack`,
  `Decreased Stat` = `special-attack`. Then `"hardy"`: all four outputs empty (neutral nature).
- **Pokemon Dimensions** (new, Geometry tab) — wire `Get Pokemon`'s `Height`/`Weight` (for
  `"pikachu"`, 4/60) into `Height`/`Weight`: `Height (m)` = `0.4`, `Weight (kg)` = `6`, `Reference
  Box` visible in the Rhino viewport as a small box at the origin.
- **Stat Mesh 3D** (new, Visualization tab) — wire `Get Pokemon`'s `Stat Values` (for `"pikachu"`)
  into `Stat Values`: `Status` = `OK`, `Mesh` renders as a closed 6-sided extruded solid in the
  viewport, with visibly uneven top heights per stat.
- **Type Palette** (new, Display tab) — `Type 1` = `"fire"`, `Type 2` = `"water"`: `Color 1` an
  orange-red swatch, `Color 2` a blue swatch, `Blend Color` a swatch roughly between the two.
- **Canvas Sprite Card** (new, Display tab) — `Name` = `"Pikachu"`, `Stats` = a text panel with
  `"hp: 35"` and `"attack: 55"` (2 items): `Status` = `OK`, `Card Mesh` a flat rectangle in the
  viewport, `Label` = `"Pikachu\nhp: 35\nattack: 55"`.
- **Batch Downloader** (new, Pokemon tab) — wire 2-3 sprite URLs (e.g. from `Get Pokemon Batch`)
  into `URLs`, `Trigger` = `false` first (`Bitmaps` empty, `Status` = `"Waiting for trigger."`),
  then `Trigger` = `true`: `Bitmaps` has one decoded image per URL, `Status` = `"OK (n/n)"`.

## v3.0.0 additions (Generative tab + Rhino Type Material)

- **Evolution Tree** (new, Generative tab) — `Species Name Or ID` = `"bulbasaur"`: `Status` = `OK`,
  `Node Names` = `[bulbasaur, ivysaur, venusaur]`, `Node Points` has 3 points with increasing
  (more negative) Y per generation, `Branch Lines` has 2 lines, `Triggers` = `[level-up, level-up]`.
  Second check with `"eevee"`: 9 nodes, 8 branch lines fanning out from eevee at even X spacing.
- **Sprite To Voxel** (new, Generative tab) — wire `Sprite Downloader`'s `Bitmap` output (for
  `pikachu`'s front sprite) into `Sprite Bitmap`, leave other inputs at defaults: `Status` = `OK`,
  `Voxel Mesh` renders as a blocky 3D silhouette of the sprite in the viewport, `Voxel Count` > 0,
  `Heightfield Mesh` renders as a relief grid.
- **Stat Growth** (new, Generative tab) — wire `Get Pokemon`'s `Stat Values` (for `pikachu`) into
  `Base Stats`, `Level` = `50`, leave IVs/EVs empty (defaults to 31/0), `Increased Stat` = `"speed"`,
  `Decreased Stat` = `"attack"`: `Status` = `OK`, `Computed Stats` has 6 values, the speed entry
  higher than attack relative to their base-stat ratio (nature applied correctly).
- **Team Synergy** (new, Generative tab) — `Names` = `["Charizard", "Blastoise"]`, `Type 1s` =
  `["fire", "water"]`, `Type 2s` = `["flying", ""]`: `Status` = `OK`, `Member`/`Attacking Type`/
  `Multiplier` each have 36 rows (2 members x 18 types), `Coverage Gaps` should be empty for this
  pair (their types don't share a common double weakness), `Composite Vulnerability` has 18 values.
- **Rhino Type Material** (new, Display tab) — `Type 1` = `"electric"`, `Add To Document` = `false`:
  `Status` = `OK`, `Material` is a non-null generic output, `Color` a yellow swatch. Then
  `Add To Document` = `true` with a Rhino document open: confirm a new "PokeData electric" material
  appears in Rhino's Materials panel.

## v4.0.0 additions (Berry / Location / Machine)

- **Get Berry** (new, Items tab) — `Berry Name Or ID` = `"cheri"`: `Status` = `OK`, `Growth Time` =
  `3`, `Max Harvest` = `5`, `Size` = `20`, `Smoothness` = `25`, `Soil Dryness` = `15`,
  `Natural Gift Power` = `60`, `Natural Gift Type` = `"fire"`, `Item Name Or ID` = `"cheri-berry"`.
  Wire `Item Name Or ID` straight into `Get Item`'s `Item Name Or ID` input (same name/type/access)
  — confirm `Get Item` resolves it with no adapter component, `Category` = `"other"` (in-battle item).
- **Get Berry Flavors** (new, Items tab) — same `Berry Name Or ID` = `"cheri"`: `Status` = `OK`,
  `Flavor Names` = `[spicy, dry, sweet, bitter, sour]` (5 entries, always all 5 flavors),
  `Potencies` = `[10, 0, 0, 0, 0]`, parallel and in the same order.
- **Get Location** (new, Data tab) — `Location Name Or ID` = `"canalave-city"`: `Status` = `OK`,
  `Name` = `"canalave-city"`, `Region` = `"sinnoh"`.
- **Get Location Areas** (new, Data tab) — same `Location Name Or ID` = `"canalave-city"`:
  `Status` = `OK`, `Area Names` = `["canalave-city-area"]` (1 entry for this location).
- **Get Machine** (new, Moves tab) — `Machine ID` = `1`: `Status` = `OK`, `Version Group` =
  `"sword-shield"`, `Move Name Or ID` = `"mega-punch"`, `Item Name Or ID` = `"tm00"`. Wire
  `Move Name Or ID` into `Get Move`'s `Move Name Or ID` input and `Item Name Or ID` into `Get
  Item`'s `Item Name Or ID` input — confirm both resolve with no adapter component, `Get Move`
  returns `Power` > 0, `Get Item` returns a non-empty `Category`.

## v4.0.0 round 5 additions (Region / Pokedex / Egg Group / Growth Rate / Stat)

- **Get Region** (new, Data tab) — `Region Name Or ID` = `"kanto"`: `Status` = `OK`,
  `Name` = `"kanto"`, `Main Generation` = `"generation-i"`.
- **Get Region Locations** (new, Data tab) — same `Region Name Or ID` = `"kanto"`: `Status` = `OK`,
  `Location Names` has many entries including `"pallet-town"`, `"viridian-city"`. Wire one entry
  into `Get Location`'s `Location Name Or ID` — confirms it resolves.
- **Get Region Pokedexes** (new, Data tab) — same `Region Name Or ID` = `"kanto"`: `Status` = `OK`,
  `Pokedex Name Or ID` = `["kanto", "letsgo-kanto"]`. Wire the first entry straight into
  `Get Pokedex`'s `Pokedex Name Or ID` input (same name/type/access) — confirm it resolves with no
  adapter component.
- **Get Pokedex** (new, Data tab) — `Pokedex Name Or ID` = `"kanto"`: `Status` = `OK`,
  `Name` = `"kanto"`, `Is Main Series` = `true`.
- **Get Pokedex Species** (new, Data tab) — same `Pokedex Name Or ID` = `"kanto"`: `Status` = `OK`,
  `Species Names` starts with `["bulbasaur", "ivysaur", "venusaur", ...]` in dex-entry order.
- **Get Egg Group** (new, Pokemon tab) — `Egg Group Name Or ID` = `"monster"`: `Status` = `OK`,
  `Name` = `"monster"`, `Species Names` includes `"bulbasaur"`, `"charmander"`. Wire
  `Get Pokemon Species`'s `Egg Groups` output (for `"bulbasaur"`, which includes `"monster"`) into
  this component's input — confirms the per-item chain.
- **Get Growth Rate** (new, Pokemon tab) — `Growth Rate Name Or ID` = `"slow"`: `Status` = `OK`,
  `Formula` is a non-empty LaTeX string, `Max Level` = `100`.
- **Get Growth Rate Levels** (new, Pokemon tab) — same `Growth Rate Name Or ID` = `"slow"`:
  `Status` = `OK`, `Levels` = `[1, 2, 3, ...]`, `Experience` = `[0, 10, 33, ...]`, parallel and in
  order — plot `Levels` vs `Experience` to confirm the curve renders.
- **Get Stat** (new, Stats tab) — `Stat Name Or ID` = `"hp"`: `Status` = `OK`,
  `Is Battle Only` = `false`. Second check with `"accuracy"`: `Is Battle Only` = `true`.
- **Get Stat Affecting Natures** (new, Stats tab) — `Stat Name Or ID` = `"attack"`: `Status` = `OK`,
  `Increasing Natures` includes `"lonely"`, `"adamant"`, `"naughty"`; `Decreasing Natures` includes
  `"bold"`. Wire one entry into `Get Nature`'s `Nature Name Or ID` input — confirms it resolves.
- **Cache check** — place `Get Region` and `Get Region Locations` on the same canvas with the same
  `Region Name Or ID`, then check the plugin's network activity (e.g. a proxy or Fiddler trace):
  only one `GET /region/{id}/` should fire, confirming the shared `PokeDataClient` response cache
  is working across both components.
  appears in Rhino's Materials panel.

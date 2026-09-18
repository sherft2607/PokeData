# Changelog

## 2.0.0 — 2026-09-18

- New **Pokemon Cry** component (Pokemon tab) — cry audio URLs plus an in-memory-download,
  trigger-to-play workflow.
- New **Type Matchup** component (Types tab) — 2x/0.5x/0x outgoing damage relations as 3
  structured lists.
- New **Visualization** tab: **Stat Radar** — normalizes base stat values into a 2D radar chart
  polyline for parametric stat-comparison geometry.
- `Get Move` gained `Priority` and `Description` (localized, one entry per language) outputs.
- `Get Ability` gained `Short Effect` (localized, one entry per language) output.
- `Get Type` gained a `Color` output — canonical elemental-type color for viewport display
  pipelines.
- Retrofit (pre-1.0.0 build catchup): Release build `PropertyGroup` confirmed present; `.yak`
  packaging re-verified for all 3 targets.
- New **Data** tab: **Get Generation** (species roster + region for a generation 1-9),
  **Pokemon Filter** (filters parallel name/value lists by rule against Min/Max).
- New **Items** tab: **Get Item** — category, cost, fling power, effect text, sprite URL.
- New **Stats** tab: **Get Nature** — +10%/-10% stat modifiers and berry-flavor preferences.
- New **Geometry** tab: **Pokemon Dimensions** — PokeAPI height/weight converted to metric +
  imperial units, plus a reference Box.
- New **Display** tab: **Type Palette** (dual-type color swatches + blend) and
  **Canvas Sprite Card** (a viewport "trading card" layout: mesh + label + plane).
- **Visualization** tab gained **Stat Mesh 3D** — a 3D extruded radar "skyline" mesh, one vertex
  height per base stat.
- **Types** tab gained **Dual Type Matchup** — a two-type Pokemon's true combined defensive
  multiplier (4x/2x/1x/0.5x/0.25x/0x) against every attacking type.
- **Pokemon** tab gained **Batch Downloader** — parallel, trigger-gated image downloads for a
  list of URLs.
- 3 new endpoints (`generation`, `item`, `nature`) validated live against pokeapi.co.

## 1.0.0 — 2026-09-17

[GitHub Release](https://github.com/sherft2607/PokeData/releases/tag/v1.0.0)

- Initial release: Pokemon, Types, Evolution, Moves, and Presets tabs.
- `Get Pokemon` / `Get Pokemon Batch` — lookup, types, stats, sprites, cry URLs.
- `Get Pokemon Species` — color, shape, habitat, capture rate, growth rate, egg groups, legendary/mythical flags.
- `Sprite Downloader` — turns a sprite/image URL into a Bitmap on the canvas.
- `Get Type` / `Get Type Matrix` — damage relations, associated Pokemon, full 18x18 matrix.
- `Get Evolution Chain` — flattened parent/child/trigger evolution edges.
- `Get Move` / `Get Ability` — mechanical fields for team-planning tools.
- `Type Name` preset — fixed dropdown of the 18 elemental types.

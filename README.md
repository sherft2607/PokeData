<!-- pr:begin header -->
<img src="PK_Plugin_Icon.png" alt="PokeData" width="96" align="left" style="margin-right:16px">

# PokeData
> Grasshopper plugin for the PokeAPI REST API — by Shandon Herft
>
> Built with the [PleaseREST](https://github.com/sherft2607/PleaseREST) Claude Code plugin — an AI pipeline that turns a REST API reference into a Grasshopper plugin.

Look up Pokemon, types, evolution chains, moves, abilities, items, berries, locations, machines,
regions, pokedexes, egg groups, growth rates, and stats from PokeAPI directly on the Grasshopper
canvas — no token, no sign-up, fully public.

<br clear="left">

## What you can do with it

- **Drive parametric geometry from real creature data** — pull a Pokemon's base stats, types,
  size, and sprite images into GH to build radar/spider diagrams or stat-sized swarms of shapes.
- **Build a full data table from a name list** — feed a list of Pokemon names in and get back a
  parallel tree of types/stats/sprites in one component, instead of one API call at a time.
- **Assemble the type-effectiveness matrix without 18 manual lookups** — one component returns
  the full 18x18 attacking/defending damage matrix.
- **Turn an evolution chain into a branching diagram** — flattened parent/child/trigger edges,
  ready for a parametric node-position layout.
- **Feed team-planning tools with move/ability mechanics** — power, accuracy, PP, priority,
  damage class, and localized descriptions, without leaving Grasshopper.
- **Hear a Pokemon's cry alongside its other data** — cry URLs plus an in-memory, trigger-to-play
  audio component, no separate download step.
- **Check type matchups at a glance** — one component returns exactly what an attacking type is
  super-effective, resisted, or immune against.
- **Turn base stats into a radar chart, colored by type** — a normalized 2D stat-radar polygon
  driven by `Get Pokemon`'s stat values and `Get Type`'s canonical type color.
- **See a dual-type Pokemon's true defenses** — one component combines two types' relations into
  the real 4x/2x/1x/0.5x/0.25x/0x multiplier per attacking type.
- **Pull a generation's whole species roster, then filter it** — one component per step, no
  hand-wiring native GH list filters for a "stat total between X and Y" style query.
- **Look up items and natures for team/breeding tools** — category, cost, fling power, and
  effect text for items; stat modifiers and berry-flavor preferences for natures.
- **Convert raw height/weight into real-world scale geometry** — metric + imperial units plus a
  placeable reference Box.
- **Extrude stats into a 3D mesh, or lay out a viewport "trading card"** — a solid stat-comparison
  shape, dual-type color swatches, and a sprite+stats card layout.
- **See an evolution chain as real tree geometry** — automatically laid-out points and branch
  lines, not just flattened edges you have to position by hand.
- **Turn a sprite into 3D geometry** — a voxel box mesh with per-pixel color, or a luminance
  heightfield relief, for generative art driven by creature artwork.
- **Compute exact battle stats** — base stats, level, IVs, EVs, and nature run through the same
  formulas the games use, for competitive team-planning tools.
- **See a whole team's defensive coverage at a glance** — a per-member x per-type heatmap,
  team-wide coverage gaps, and a composite vulnerability score, instead of cross-referencing 6
  type charts by hand.
- **Get a real render material colored by type** — a Rhino `RenderMaterial` built from canonical
  type colors, ready to shade a generated model.
- **Look up berries, locations, and TM/HM machines, chained straight into existing lookups** — a
  berry's item, or a machine's move and item, come out named to match `Get Item`'s and `Get
  Move`'s inputs exactly, so they wire straight in with no adapter component.
- **Drill from a region down to its species roster, or a stat to the natures that shape it** — a
  region's pokedexes chain into a full dex species list, a species' egg group/growth rate chain
  into that group's members or that rate's level curve, and a stat chains to the natures that
  raise or lower it — every step behind a shared response cache, so a lookup and its list
  breakout never double-fetch the same endpoint.

Component chains for each of these are in [docs/workflows.md](docs/workflows.md); worked examples
are in [docs/examples/](docs/examples/).

**Under the hood**

- PokeAPI is fully public — there is no Auth tab and no `Token` input on any component.
- Every lookup component accepts either a name or a numeric ID in the same input.
<!-- pr:end header -->

<!-- pr:begin install -->
## Installation

**Food4Rhino:** `(pending listing)`
**Yak:** `_PackageManager` → search `pokedata`

**Rhino 8 note:** the Rhino 8 packages are built for .NET 7 and load only when Rhino runs on the
.NET Core runtime (its default). If the PokeData tab is missing, run `SetDotNetRuntime` in Rhino,
choose **.NET Core**, and restart.

**From source:** `dotnet build PokeData.csproj`, then drop the `.gha` for your Rhino version into
Grasshopper's `Libraries` folder (Rhino 7: `bin/Debug/net48`; Rhino 8 Windows:
`bin/Debug/net7.0-windows`; Rhino 8 Mac: `bin/Debug/net7.0`) and restart Rhino — or point
Grasshopper's developer-folder setting at the build folder. Close Rhino before rebuilding; it
locks the `.gha`.
<!-- pr:end install -->

<!-- pr:begin quickstart -->
## Quick start

1. Install PokeData via Rhino's `_PackageManager` — no token needed.
2. Open Grasshopper, find the **PokeData** tab.
3. Drop **Get Pokemon** onto the canvas, wire a text panel with `"pikachu"` into `Pokemon Name Or ID`.
4. Check `Status` reads `OK`.
5. Explore the other tabs: **Types** for damage matchups, **Evolution** for evolution chains,
   **Moves** for move/ability lookups.

Full walkthrough: [docs/quickstart.md](docs/quickstart.md). Common wirings as component chains:
[docs/workflows.md](docs/workflows.md). Reference for every component: [docs/index.md](docs/index.md).
<!-- pr:end quickstart -->

<!-- pr:begin components -->
## Components

| Tab | Component | Inputs | Outputs | Description |
|---|---|---|---|---|
| Pokemon | Get Pokemon | Pokemon Name Or ID | Types, Height, Weight, Base Experience, Stat Names, Stat Values, Sprite Front URL, Sprite Artwork URL, Status, Response, Legacy Cry URL, Latest Cry URL | Single Pokemon lookup |
| Pokemon | Get Pokemon Batch | Pokemon Names Or IDs | (same fields, as a tree) | Batch lookup for a list of Pokemon |
| Pokemon | Get Pokemon Species | Species Name Or ID | Color, Shape, Habitat, Capture Rate, Base Happiness, Growth Rate, Egg Groups, Is Legendary, Is Mythical, Status, Response | Species classification detail |
| Pokemon | Sprite Downloader | Sprite URL | Bitmap, Status | Downloads an image URL to a Bitmap |
| Pokemon | Pokemon Cry | Pokemon Name Or ID, Play | Legacy Cry URL, Latest Cry URL, Status | Cry audio URLs + trigger-to-play |
| Pokemon | Batch Downloader | URLs, Trigger | Bitmaps, Status | Parallel image downloads, trigger-gated |
| Pokemon | Get Egg Group | Egg Group Name Or ID | Name, Species Names, Status, Response | Egg group member roster |
| Pokemon | Get Growth Rate | Growth Rate Name Or ID | Formula, Max Level, Status, Response | Growth rate formula lookup |
| Pokemon | Get Growth Rate Levels | Growth Rate Name Or ID | Levels, Experience, Status, Response | Level/experience curve |
| Types | Get Type | Type Name | Double/Half/No Damage To/From, Status, Response, Associated Pokemon, Color | One type's damage relations |
| Types | Get Type Matrix | (none) | Attacking Type, Defending Type, Multiplier, Status, Response | Full 18x18 type matrix |
| Types | Type Matchup | Attacking Type | 2x/0.5x/0x Damage To, Status, Response | Outgoing damage relations only |
| Types | Dual Type Matchup | Type 1, Type 2 | 4x/2x/1x/0.5x/0.25x/0x Damage From, Status, Response | True dual-type defensive multipliers |
| Evolution | Get Evolution Chain | Species Name Or ID | Parent Species, Child Species, Trigger, Status, Response | Flattened evolution chain |
| Moves | Get Move | Move Name Or ID | Power, Accuracy, PP, Damage Class, Type, Effect Text, Status, Response, Priority, Description | Move mechanics lookup |
| Moves | Get Ability | Ability Name Or ID | Generation, Effect Text, Status, Response, Short Effect | Ability mechanics lookup |
| Moves | Get Machine | Machine ID | Version Group, Move Name Or ID, Item Name Or ID, Status, Response | TM/HM lookup, chains into Get Move/Get Item |
| Items | Get Item | Item Name Or ID | Category, Cost, Fling Power, Effect Text, Sprite URL, Status, Response | Item lookup |
| Items | Get Berry | Berry Name Or ID | Name, Growth Time, Max Harvest, Size, Smoothness, Soil Dryness, Natural Gift Power, Natural Gift Type, Item Name Or ID, Status, Response | Berry lookup, chains into Get Item |
| Items | Get Berry Flavors | Berry Name Or ID | Flavor Names, Potencies, Status, Response | Berry flavor/potency breakdown |
| Stats | Get Nature | Nature Name Or ID | Increased/Decreased Stat, Liked/Disliked Flavor, Status, Response | Nature lookup |
| Stats | Get Stat | Stat Name Or ID | Is Battle Only, Status, Response | Stat battle-only flag |
| Stats | Get Stat Affecting Natures | Stat Name Or ID | Increasing/Decreasing Natures, Status, Response | Reverse nature lookup |
| Data | Get Generation | Generation | Species IDs, Species Names, Region, Status, Response | Generation species roster |
| Data | Pokemon Filter | Names, Values, Filter Rule, Min, Max | Filtered Names, Filtered Values, Match Indices, Status | Parallel-list filtering |
| Data | Get Location | Location Name Or ID | Name, Region, Status, Response | Location lookup |
| Data | Get Location Areas | Location Name Or ID | Area Names, Status, Response | Location sub-area names |
| Data | Get Region | Region Name Or ID | Name, Main Generation, Status, Response | Region lookup |
| Data | Get Region Locations | Region Name Or ID | Location Names, Status, Response | Region location names |
| Data | Get Region Pokedexes | Region Name Or ID | Pokedex Name Or ID, Status, Response | Region pokedex names |
| Data | Get Pokedex | Pokedex Name Or ID | Name, Is Main Series, Status, Response | Pokedex lookup |
| Data | Get Pokedex Species | Pokedex Name Or ID | Species Names, Status, Response | Full dex species roster |
| Geometry | Pokemon Dimensions | Height, Weight, Plane | Height (m/ft), Weight (kg/lb), Reference Box, Status | Unit conversion + reference Box |
| Visualization | Stat Radar | Stat Values, Radius, Max Stat, Center | Radar Points, Radar Polyline, Status | Normalized 2D stat radar chart |
| Visualization | Stat Mesh 3D | Stat Values, Height Factor, Radius, Max Stat, Center | Mesh, Status | Extruded 3D radar mesh |
| Display | Type Palette | Type 1, Type 2 | Color 1, Color 2, Blend Color, Status | Dual-type color swatches |
| Display | Canvas Sprite Card | Name, Stats, Sprite Bitmap, Point, Width, Height | Card Mesh, Label, Card Plane, Sprite Bitmap, Status | Viewport card layout |
| Display | Rhino Type Material | Type 1, Type 2, Add To Document | Material, Color, Status | Diffuse RenderMaterial from type color(s) |
| Generative | Evolution Tree | Species Name Or ID, Horizontal/Vertical Spacing, Origin | Node Names, Node Points, Branch Lines, Triggers, Status, Response | Evolution chain as tree geometry |
| Generative | Sprite To Voxel | Sprite Bitmap, Voxel Size, Alpha Threshold, Heightfield Scale, Max Resolution | Voxel Mesh, Heightfield Mesh, Voxel Count, Status | Voxelizes a sprite into 3D geometry |
| Generative | Stat Growth | Base Stats, Level, IVs, EVs, Increased/Decreased Stat | Computed Stats, Status | Exact runtime battle stats |
| Generative | Team Synergy | Names, Type 1s, Type 2s | Member, Attacking Type, Multiplier, Coverage Gaps, Composite Vulnerability, Status | Team defensive heatmap + gaps |
| Presets | Type Name | (none) | Type Name | Fixed dropdown of the 18 elemental types |
<!-- pr:end components -->

<!-- pr:begin requirements -->
## Requirements

- Rhino 7 or 8 (Windows or Mac)
- No API account, no token — PokeAPI is fully public

## Documentation

- [docs/index.md](docs/index.md) — overview and every component by tab
- [docs/quickstart.md](docs/quickstart.md) — first lookup in a few steps
- [docs/workflows.md](docs/workflows.md) — common wirings as component chains
- [docs/examples/](docs/examples/) — worked examples
- [docs/changelog.md](docs/changelog.md)

## Development

```
dotnet build PokeData.csproj                          # net48, net7.0-windows, net7.0
dotnet test  PokeData.Tests/PokeData.Tests.csproj      # pure parsing logic, no Rhino needed
```

This plugin was generated with the **PleaseREST** Claude Code plugin (`please-rest` v1.0.0);
`CLAUDE.md` holds the build state and design notes.
<!-- pr:end requirements -->

<!-- pr:begin license -->
## License

PokeData is released under the MIT License — Copyright (c) 2026 Shandon Herft. See [LICENSE](LICENSE).
<!-- pr:end license -->

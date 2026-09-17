<!-- pr:begin header -->
<img src="PK_Plugin_Icon.png" alt="PokeData" width="96" align="left" style="margin-right:16px">

# PokeData
> Grasshopper plugin for the PokeAPI REST API — by Shandon Herft
>
> Built with the [PleaseREST](https://github.com/sherft2607/PleaseREST) Claude Code plugin — an AI pipeline that turns a REST API reference into a Grasshopper plugin.

Look up Pokemon, types, evolution chains, moves, and abilities from PokeAPI directly on the
Grasshopper canvas — no token, no sign-up, fully public.

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
- **Feed team-planning tools with move/ability mechanics** — power, accuracy, PP, damage class,
  and effect text, without leaving Grasshopper.

Component chains for each of these are in [docs/workflows.md](docs/workflows.md); worked examples
are in [docs/examples/](docs/examples/).

**Under the hood**

- PokeAPI is fully public — there is no Auth tab and no `Token` input on any component.
- Every lookup component accepts either a name or a numeric ID in the same input.
<!-- pr:end header -->

<!-- pr:begin install -->
## Installation

**Food4Rhino:** `(pending first release)`
**Yak:** `_PackageManager` → search `pokedata` `(pending first release)`

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
| Types | Get Type | Type Name | Double/Half/No Damage To/From, Status, Response, Associated Pokemon | One type's damage relations |
| Types | Get Type Matrix | (none) | Attacking Type, Defending Type, Multiplier, Status, Response | Full 18x18 type matrix |
| Evolution | Get Evolution Chain | Species Name Or ID | Parent Species, Child Species, Trigger, Status, Response | Flattened evolution chain |
| Moves | Get Move | Move Name Or ID | Power, Accuracy, PP, Damage Class, Type, Effect Text, Status, Response | Move mechanics lookup |
| Moves | Get Ability | Ability Name Or ID | Generation, Effect Text, Status, Response | Ability mechanics lookup |
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

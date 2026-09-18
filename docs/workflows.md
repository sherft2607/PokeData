# Workflows

What you can do with PokeData:

- Pull a Pokemon's base stats, types, size, and sprite images to drive a radar chart or stat-sized
  swarm of geometry — see [Pokemon lookup](examples/pokemon-lookup.md)
- Feed a list of Pokemon names in and get back a full parallel data table (types, stats, sprites)
  in one component — see [Batch Pokedex table](examples/pokemon-batch-table.md)
- Build the entire 18x18 type-effectiveness matrix in one placed component instead of 18 manual
  lookups — see [Type effectiveness matrix](examples/type-matrix.md)
- Flatten a species' full evolution chain into parent/child/trigger edges for a parametric
  branching diagram — see [Evolution chain graph](examples/evolution-chain.md)
- Pull a move or ability's mechanical fields for a team-planning or damage-calculation graph — see
  [Move and ability lookup](examples/moves-abilities.md)
- Look up a species' color, shape, habitat, capture rate, and legendary/mythical flags
- Turn a sprite image URL into an actual `Bitmap` on the canvas for image samplers
- Hear a Pokemon's cry alongside its other data, in-memory, no separate download step (v2.0.0)
- Look up which types are super-effective, resisted, or immune against a given attacking type,
  without reading the full to/from matrix (v2.0.0)
- Turn base stat values into a normalized 2D radar/spider chart polygon for parametric
  stat-comparison geometry, colored by the type's canonical color (v2.0.0)
- See a two-type Pokemon's true combined defensive multiplier against every attacking type,
  not just one type's own relations (v2.0.0)
- Pull every species introduced in a given generation, then filter a Pokedex-scale list down to
  matches by a numeric rule, without hand-wiring native GH list filters (v2.0.0)
- Look up an item's category/cost/fling power/effect, or a nature's stat modifiers and
  berry-flavor preferences, for team-building and EV-planning tools (v2.0.0)
- Convert a Pokemon's raw height/weight into metric+imperial units and a placeable reference Box,
  for real-world-scale parametric geometry (v2.0.0)
- Extrude base stats into a solid 3D radar mesh, get dual-type color swatches, or lay out a
  sprite + stats as a viewport "trading card" (v2.0.0)
- See a species' evolution chain as actual placed tree geometry — points and branch lines laid
  out automatically — instead of flattened edges you position by hand (v3.0.0)
- Turn a sprite image into real 3D geometry: a voxel box mesh with per-pixel color, or a
  luminance heightfield relief, for generative art driven by creature artwork (v3.0.0)
- Compute a Pokemon's exact runtime battle stats from base stats/level/IVs/EVs/nature — the same
  formulas the games use — for competitive team-planning tools (v3.0.0)
- See a full 1-6 Pokemon team's defensive coverage at a glance: a per-member x per-type heatmap,
  team-wide coverage gaps, and a composite vulnerability score per type (v3.0.0)
- Get a real Rhino render material colored by a Pokemon's type(s), for render/viewport-shading
  a generated model by type (v3.0.0)

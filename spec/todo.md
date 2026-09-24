# PokeData — Build TODO

> Deviation from the standard template: PokeAPI has no auth and no write endpoints, so build
> Phase 1 has no Auth component, no ButtonComponent, and no Auth icon — just the client, info,
> and utilities scaffolding. Build Phase 2 has no JSON Builders (no request bodies to construct —
> every endpoint is a plain GET). Build Phase 7's test file is `ParsingTests.cs`, not
> `BuilderTests.cs` — it covers the flattening logic in `GetTypeMatrixComponent` and
> `GetEvolutionChainComponent`, the only components with non-trivial logic beyond an HTTP call.

```
PHASE 1 — CLIENT + SCAFFOLD
[x] PokeDataClient.cs              [CODE] base HTTP GET helper, base URL https://pokeapi.co/api/v2, no auth headers
[x] PokeDataInfo.cs                [CODE] GH_AssemblyInfo + GH_AssemblyPriority
[x] PluginUtilities.cs             [CODE] TabName "PokeData" + subcategory constants (Pokemon, Types, Evolution, Moves, Presets, Utilities)
[ ] Verify: dotnet build succeeds, client can GET a known endpoint (e.g. /pokemon/pikachu) and parse JSON

PHASE 2 — SIMPLE UNITS (GET components)
[x] GetPokemon.cs                  [CODE] Pokemon Name Or ID -> Types, Height, Weight, Base Experience, Stat Names, Stat Values, Sprite Front URL, Sprite Artwork URL, Status, Response
[x] Pokemon icon                   [ASSET] generate_icon -> add_to_resources
[x] GetPokemonBatch.cs             [CODE] Pokemon Names Or IDs (list) -> same fields as tree, one branch per input
[x] Pokemon Batch icon             [ASSET] generate_icon -> add_to_resources
[x] GetType.cs                     [CODE] Type Name -> Double/Half/No Damage To/From (6 lists), Status, Response
[x] Type icon                      [ASSET] generate_icon -> add_to_resources
[x] GetMove.cs                     [CODE] Move Name Or ID -> Power, Accuracy, PP, Damage Class, Type, Effect Text, Status, Response
[x] Move icon                      [ASSET] generate_icon -> add_to_resources
[x] GetAbility.cs                  [CODE] Ability Name Or ID -> Generation, Effect Text, Status, Response
[x] Ability icon                   [ASSET] generate_icon -> add_to_resources
[ ] Verify: each component returns valid parsed data for a known name/ID from the live API

PHASE 3 — CONSOLIDATED COMPONENTS (internal multi-call orchestration, no separate aggregator tier)
[x] GetTypeMatrix.cs               [CODE] no inputs -> lists all 18 types, fetches each, flattens damage_relations into Attacking/Defending/Multiplier parallel lists
[x] Type Matrix icon               [ASSET] generate_icon -> add_to_resources
[x] GetEvolutionChain.cs           [CODE] Species Name Or ID -> resolves species -> evolution_chain URL internally, fetches chain, flattens into Parent/Child/Trigger parallel lists
[x] Evolution Chain icon           [ASSET] generate_icon -> add_to_resources
[ ] Verify: matrix returns 324 rows (18x18), evolution chain returns correct edges for a multi-stage species (e.g. bulbasaur)

PHASE 4 — LEVEL 2 AGGREGATOR
SKIPPED — no higher-order structures beyond what Phase 3's consolidated components already cover

PHASE 5 — CUSTOM / AEC FEATURES
SKIPPED — no AEC features selected at Gate 3

PHASE 6 — PRESETS
[x] PokemonTypePreset.cs           [CODE] GH_ValueList of the 18 elemental type names -> Type Name output
[x] Preset icon                    [ASSET] generate_icon -> add_to_resources
[ ] Verify: preset output wires directly into GetType.cs's Type Name input

PHASE 7 — TESTS
[x] PokeData.Tests/ParsingTests.cs [TEST] one test per non-trivial parsing/flattening method: type-matrix flattening (18x18 = 324 rows, correct multiplier per cell), evolution-chain flattening (parent/child/trigger edges, multi-stage chain), stat name/value pairing order
[x] dotnet build                   [CODE] all green
[x] dotnet test                    [CODE] all green
[DEGRADED — no Grasshopper MCP bridge available this session; see dogfood F6]
[x] demos/README.md                [MCP]  manual build spec for all 5 demos (goal/components/wiring/inputs/expected), written in place of live .gh files
[ ] demos/pokemon-lookup.gh        [MANUAL] build from demos/README.md section 1, confirm at Gate 6
[ ] demos/pokemon-batch-table.gh   [MANUAL] build from demos/README.md section 2, confirm at Gate 6
[ ] demos/type-matrix.gh           [MANUAL] build from demos/README.md section 3, confirm at Gate 6
[ ] demos/evolution-chain.gh       [MANUAL] build from demos/README.md section 4, confirm at Gate 6
[ ] demos/moves-abilities.gh       [MANUAL] build from demos/README.md section 5, confirm at Gate 6
[x] Verify: demos/ is populated (degraded to manual — README.md in place of live .gh files, per pr-rhino-test's documented fallback)

PHASE 8 — DOCS                     (pipeline Phase 13 — after Gate 6)
[ ] docs/index.md                  [SHIP] plugin overview + install
[ ] docs/quickstart.md             [SHIP]
[ ] docs/components/Pokemon/GetPokemon.md       [SHIP]
[ ] docs/components/Pokemon/GetPokemonBatch.md  [SHIP]
[ ] docs/components/Types/GetType.md            [SHIP]
[ ] docs/components/Types/GetTypeMatrix.md      [SHIP]
[ ] docs/components/Evolution/GetEvolutionChain.md [SHIP]
[ ] docs/components/Moves/GetMove.md            [SHIP]
[ ] docs/components/Moves/GetAbility.md         [SHIP]
[ ] docs/components/Presets/PokemonTypePreset.md [SHIP]
[ ] docs/examples/[demo].md        [SHIP] one per demo workflow
[ ] docs/changelog.md              [SHIP] starts at 1.0.0
(no docs/authentication.md — API has no auth)

PHASE 9 — GITHUB                   (pipeline Phase 14)
[ ] README.md                      [SHIP] may link to docs/
[ ] CLAUDE.md                      [SHIP]
[ ] LICENSE                        [SHIP]
[ ] .gitignore                     [SHIP]

PHASE 10 — PACKAGE                 (pipeline Phase 15)
[ ] dotnet build -c Release x 3    [SHIP] net48 / net7.0-windows / net7.0
[ ] yak build x 3                  [SHIP]
[ ] .github/workflows/publish.yml  [SHIP]
[ ] Verify: .yak files in dist/

[HUMAN GATE 6] Rhino manual test — demos pre-built & pre-checked; open demos/ and confirm live in GH
[HUMAN GATE 7] Cross-platform test — install .yak on Rh7 Win / Rh8 Win / Rh8 Mac
```

Note: Gate 5 (auth test in Rhino) is skipped for this build — there is no Auth component to test.

## v2.0.0 extend round (rest-add-feature)

[x] PokemonCry.cs               [BUILD] Pokemon tab — cry URLs + trigger-to-play
[x] TypeMatchup.cs              [BUILD] Types tab — 2x/0.5x/0x outgoing damage lists
[x] StatRadar.cs                [BUILD] new Visualization tab — normalized radar polygon
[x] GetMove.cs                  [BUILD] +Priority, +Description outputs (appended)
[x] GetAbility.cs               [BUILD] +Short Effect output (appended)
[x] GetType.cs                  [BUILD] +Color output (appended)
[x] PokeDataParsing.cs          [BUILD] +ParseLocalizedEntries, +ComputeStatRadarPoints
[x] PluginUtilities.cs          [BUILD] +TypeColor canonical palette, +Visualization category
[x] icons.json + Icons/*.png    [MCP]   3 new icons rendered + embedded, user-approved
[x] PokeData.Tests/ParsingTests.cs [BUILD] +18 tests for new pure-logic helpers (40/40 passing)
[x] dotnet build x 3 targets    [BUILD] net48 / net7.0-windows / net7.0 all green
[ ] demos/README.md v2.0.0 additions [MANUAL] confirm live in Rhino at Gate 6 re-test
[x] docs/ incremental update    [SHIP] new component pages + index/workflows/changelog entries
[x] PokeData.csproj Version -> 2.0.0
[x] Release PropertyGroup catchup verified present (pre-1.0.0 retrofit item)
[ ] README.md / CLAUDE.md regenerated for v2.0.0 (pr-gh-package)
[ ] /rest-package rebuild for v2.0.0 (3 .yak in dist/)
[ ] git commit + tag v2.0.0 + push (user's call — offered, not auto-run)

## v2.1.0 extend round (rest-add-feature — 10-component batch)

[x] GetGeneration.cs             [BUILD] Data tab — species roster + region (new endpoint: generation_read)
[x] DualTypeMatchup.cs           [BUILD] Types tab — dual-type defensive multiplier buckets
[x] PokemonDimensions.cs         [BUILD] new Geometry tab — unit conversion + reference Box
[x] StatMesh3D.cs                [BUILD] Visualization tab — extruded 3D radar mesh
[x] GetItem.cs                   [BUILD] new Items tab (new endpoint: item_read)
[x] TypePalette.cs               [BUILD] new Display tab — type color swatches + blend
[x] PokemonFilter.cs             [BUILD] Data tab — parallel-list filter by rule
[x] GetNature.cs                 [BUILD] new Stats tab (new endpoint: nature_read)
[x] CanvasSpriteCard.cs          [BUILD] Display tab — card mesh + label + plane
[x] BatchDownloader.cs           [BUILD] Pokemon tab — parallel async image downloads
[x] PokeDataParsing.cs           [BUILD] +10 pure helpers (Ids-from-url, dual-type defense,
    dimension conversion, regular-polygon points, stat-mesh heights, filter-by-rule,
    card label join, batch status summary)
[x] PluginUtilities.cs           [BUILD] +AllTypeNames, +BlendColors; 5 new subcategory constants
    (Items, Stats, Data, Geometry, Display); renumbered all 12 tabs
[x] call-library.json / call-library-validated.json [MANUAL] generation_read/item_read/nature_read
    extracted + VALIDATED against live pokeapi.co this session (200s confirmed, 404 confirmed
    for a bad item ID)
[x] icons.json + Icons/*.png     [MCP]   10 new icons rendered + embedded, user-approved
[x] PokeData.Tests/ParsingTests.cs [BUILD] +36 tests for new pure-logic helpers (76/76 passing)
[x] dotnet build x 3 targets     [BUILD] net48 / net7.0-windows / net7.0 all green
[ ] demos/README.md v2.1.0 additions [MANUAL] confirm live in Rhino at Gate 6 re-test (pending,
    same as v2.0.0's outstanding Gate 6 re-test)
[ ] docs/ incremental update for v2.1.0
[ ] README.md / CLAUDE.md regenerated for v2.1.0
[ ] /rest-package rebuild for v2.1.0 (3 .yak in dist/)
[ ] git commit + tag + push (user's call)

## v3.0.0 extend round (rest-add-feature — generative geometry / battle optimization)

[x] EvolutionTree.cs             [BUILD] new Generative tab — tree layout points+lines+triggers
[x] SpriteToVoxel.cs             [BUILD] Generative tab — voxel box mesh + luminance heightfield
[x] StatGrowth.cs                [BUILD] Generative tab — exact battle stat formulas
[x] TeamSynergy.cs               [BUILD] Generative tab — 6x18 heatmap + coverage gaps
[x] RhinoTypeMaterial.cs         [BUILD] Display tab — simple diffuse RenderMaterial
[x] PokeDataParsing.cs           [BUILD] +ComputeDefenseMultipliers (shared refactor),
    +ComputeTeamSynergy, +ComputeHpStat/ComputeBattleStat/NatureMultiplierFor,
    +ComputeEvolutionTreeLayout, +Luminance/IsOpaquePixel
[x] PluginUtilities.cs           [BUILD] +Generative category (13 tabs total)
[x] icons.json + Icons/*.png     [MCP]   5 new icons rendered + embedded, user-approved
[x] PokeData.Tests/ParsingTests.cs [BUILD] +30 tests for new pure-logic helpers (106/106 passing)
[x] dotnet build x 3 targets     [BUILD] net48 / net7.0-windows / net7.0 all green
    (RenderMaterial API confirmed compiling on all 3 targets, incl. net7.0 Mac target)
[x] PokeData.csproj / manifest.yml Version -> 3.0.0
[ ] docs/ incremental update for v3.0.0
[ ] README.md / CLAUDE.md regenerated for v3.0.0
[ ] demos/README.md v3.0.0 additions + Gate 6 live re-test (pending, no Grasshopper MCP bridge)
[ ] /rest-package rebuild for v3.0.0 (3 .yak in dist/)
[ ] git commit + tag v3.0.0 + push (user's call)

## v3.0.0 post-round fix

[x] Components/Generative/SpriteToVoxel.cs [FIX] Sprite Bitmap input wasn't unwrapping
    GH_ObjectWrapper before casting to Bitmap (generic params box non-IGH_Goo values;
    DA.GetData(ref object) doesn't auto-unwrap) — always read as null, threw "Sprite Bitmap is
    empty or not a Bitmap." on any real wired input. Fixed by checking for
    Grasshopper.Kernel.Types.GH_ObjectWrapper and reading .Value first. Verified Sprite
    Downloader (producer) and Canvas Sprite Card (untouched passthrough) were already correct.
    Rebuilt + tested all 3 targets, 106/106 unit tests still passing (no pure-logic change).

## v4.0.0 extend round (rest-add-feature — berry / location / machine)

[x] call-library.json / call-library-validated.json [MCP] extract + validate 3 new endpoints:
    berry_read, location_read, machine_read (merge into existing files, do not rewrite entries;
    live GET /berry/1, /location/1, /machine/1 all 200 this session)
[x] GetBerry.cs                  [BUILD] new Items tab entry — Berry Name Or ID -> growth/size/
    smoothness/soil-dryness/gift stats + Item Name Or ID (chains into GetItem.cs)
[x] GetBerryFlavors.cs           [BUILD] Items tab — Berry Name Or ID -> Flavor Names/Potencies
    parallel lists (reuses berry_read, cached, no re-fetch)
[x] GetLocation.cs               [BUILD] Data tab — Location Name Or ID -> Name, Region
[x] GetLocationAreas.cs          [BUILD] Data tab — Location Name Or ID -> Area Names list
    (reuses location_read, cached, no re-fetch)
[x] GetMachine.cs                [BUILD] Moves tab — Machine ID -> Version Group, Move Name Or ID
    (chains into GetMove.cs), Item Name Or ID (chains into GetItem.cs)
[x] PokeDataParsing.cs           [BUILD] +ParseBerryFlavors; location areas[] reuses existing
    NamesToList helper (testable, no RhinoCommon reference)
[x] icons.json + Icons/*.png     [MCP]   5 new icons rendered + embedded, user-approved
[x] PokeData.Tests/ParsingTests.cs [BUILD] +4 tests for new pure-logic parsing helpers (110/110 passing)
[x] dotnet build x 3 targets     [BUILD] net48 / net7.0-windows / net7.0 all green
[x] dotnet test                  [BUILD] all green
[x] demos/README.md v4.0.0 additions + Gate 6 live re-test — confirmed by plugin author live in
    Rhino: all 5 components (Get Berry, Get Berry Flavors, Get Location, Get Location Areas,
    Get Machine) compute with OK status; Get Berry's Item Name Or ID and Get Machine's
    Move Name Or ID / Item Name Or ID chain directly into Get Item / Get Move with zero issues
[x] docs/ incremental update for v4.0.0
[x] PokeData.csproj Version -> 4.0.0 + docs/changelog.md entry
[x] README.md / CLAUDE.md regenerated for v4.0.0
[x] /rest-package rebuild for v4.0.0 (3 .yak in dist/)
[x] git commit + tag v4.0.0 + push — done as part of round 5's finalization below

## v4.0.0 round 5 (rest-add-feature — region / pokedex / egg group / growth rate / stat)

[x] call-library.json / call-library-validated.json [MCP] extract + validate 5 new endpoints:
    region_read, pokedex_read, egg-group_read, growth-rate_read, stat_read (live GET /region/1,
    /pokedex/2, /egg-group/1, /growth-rate/1, /stat/1 + /stat/2 all 200 this session)
[x] GetRegion.cs               [BUILD] Data tab — Region Name Or ID -> Name, Main Generation
    (chains off Get Location's Region output)
[x] GetRegionLocations.cs      [BUILD] Data tab — Region Name Or ID -> Location Names list
    (reuses region_read via shared response cache, no re-fetch)
[x] GetRegionPokedexes.cs      [BUILD] Data tab — Region Name Or ID -> Pokedex Name Or ID list
    (reuses region_read via shared response cache; chains into GetPokedex.cs)
[x] GetPokedex.cs              [BUILD] Data tab — Pokedex Name Or ID -> Name, Is Main Series
[x] GetPokedexSpecies.cs       [BUILD] Data tab — Pokedex Name Or ID -> Species Names list
    (reuses pokedex_read via shared response cache; nested pokemon_entries[].pokemon_species.name)
[x] GetEggGroup.cs             [BUILD] Pokemon tab — Egg Group Name Or ID -> Name, Species Names
    (chains off Get Pokemon Species' Egg Groups list, per-item)
[x] GetGrowthRate.cs           [BUILD] Pokemon tab — Growth Rate Name Or ID -> Formula, Max Level
    (chains off Get Pokemon Species' Growth Rate output)
[x] GetGrowthRateLevels.cs     [BUILD] Pokemon tab — Growth Rate Name Or ID -> Levels/Experience
    parallel lists (reuses growth-rate_read via shared response cache, no re-fetch)
[x] GetStat.cs                 [BUILD] Stats tab — Stat Name Or ID -> Is Battle Only
    (chains off Get Nature's Increased/Decreased Stat and Get Pokemon's Stat Names)
[x] GetStatAffectingNatures.cs [BUILD] Stats tab — Stat Name Or ID -> Increasing/Decreasing
    Natures lists (reuses stat_read via shared response cache; reverse lookup of Get Nature)
[x] PokeDataClient.cs          [BUILD] +ConcurrentDictionary response cache keyed by request URL,
    shared statically across every component instance — a "core lookup + list breakout" pair
    calling the same endpoint for the same ID now only hits the network once (benefits every
    existing endpoint too, not just this round's)
[x] PokeDataParsing.cs         [BUILD] +PokedexSpeciesNames, +ParseGrowthRateLevels,
    +ParseStatAffectingNatures; region/egg-group species lists reuse existing NamesToList
[x] icons.json + Icons/*.png   [MCP]   9 new icons rendered + embedded, user-approved
[x] PokeData.Tests/ParsingTests.cs [BUILD] +9 tests for new pure-logic parsing helpers (119/119 passing)
[x] dotnet build x 3 targets   [BUILD] net48 / net7.0-windows / net7.0 all green, 0 errors
[x] dotnet test                [BUILD] all green
[x] demos/README.md round-5 additions + Gate 6 live re-test — confirmed by plugin author live in
    Rhino: all 9 components compute correctly
[x] docs/ incremental update for round 5
[x] PokeData.csproj Version -> 4.0.0 (round 5 folds into the same unreleased v4.0.0, same pattern
    as v2.0.0's batch 2) + docs/changelog.md entry
[x] README.md / CLAUDE.md regenerated for round 5
[x] Post-Gate-6 fix: PluginUtilities.cs subcategory prefixes changed to two-digit zero-padded
    ("01." through "13.") — Grasshopper ribbon was sorting 1/10/11/12/13/2/3/... alphabetically;
    confirmed by plugin author to now sort correctly in Grasshopper; rebuilt x 3 targets,
    119/119 tests still green (no logic change)
[x] /rest-package rebuild for v4.0.0 with round-5 components + ribbon fix (3 .yak in dist/)
[x] git commit ("v4.0.0: 14 new components, shared response cache, geographic hierarchy, battle
    math, and zero-padded ribbon categories") + tag v4.0.0 + push to origin/main
[x] yak login + yak push all 3 v4.0.0 packages — confirmed live on yak.rhino3d.com/packages/pokedata
[x] Backfill: docs/index.md Released line links v4.0.0's tag (no longer "not yet tagged/pushed");
    docs/changelog.md gained the GitHub Release + Yak Package Manager links (v3.0.0's pattern);
    README.md's Yak install line was already generic wording, no placeholder to replace;
    Food4Rhino listing still pending — not part of this round
[ ] Food4Rhino listing (plugin author's next step, whenever they choose to do it)
[ ] Cross-platform Gate 7 re-test on Rh7 Windows / Rh8 Mac (still only tested on Rh8 Windows,
    same open item carried since v3.0.0)

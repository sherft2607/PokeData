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

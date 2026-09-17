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

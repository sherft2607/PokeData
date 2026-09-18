<!-- pr:begin header -->
# CLAUDE.md — PokeData

Build state and design notes for this PleaseREST-generated plugin. Generated sections are
regenerated every phase/gate — text outside the `pr:begin`/`pr:end` markers is yours and is
never touched.

Generated with PleaseREST v1.0.0. Skills used: pr-conventions, pr-spec, pr-aec-advisor, pr-todo,
pr-client, pr-utilities, pr-component, pr-preset, pr-file-structure, pr-vs-project, pr-assembly,
pr-icons, pr-unit-tests, pr-build, pr-rhino-test, pr-docs, pr-gh-package, pr-permissions,
pr-dogfood, pr-session. MCPs used: pr-api-extractor, pr-api-tester, pr-icon-generator.
<!-- pr:end header -->

<!-- pr:begin state -->
## Current build state

- Phase: 15 of 15 — Package + Ship (v2.0.0 round in progress)
- Gates passed: 1 (API overview), 2 (structure), 3 (AEC features — none), 4 (auto-cleared — 0
  FAILED/PARTIAL calls), 5 (auto-cleared — no auth to test), 6 (manual live test — initial build,
  extend-round-1 Batch 1, v2.0.0 re-test pending on the plugin author's machine — no Grasshopper
  MCP bridge in the building sandbox)
- Extend rounds: round 1 / Batch 1 — added cries outputs to Get Pokemon, Associated Pokemon output
  to Get Type, new Get Pokemon Species and Sprite Downloader components. v2.0.0 round (2 batches,
  same unreleased version — batch 2 expanded scope before the first v2.0.0 tag) — batch 1: Pokemon
  Cry, Type Matchup, Stat Radar (new Visualization tab); Get Move +Priority/Description, Get
  Ability +Short Effect, Get Type +Color. Batch 2 (10 components): Get Generation + Pokemon Filter
  (new Data tab), Get Item (new Items tab), Get Nature (new Stats tab), Pokemon Dimensions (new
  Geometry tab), Dual Type Matchup (Types tab), Stat Mesh 3D (Visualization tab), Type Palette +
  Canvas Sprite Card (new Display tab), Batch Downloader (Pokemon tab) — 3 new endpoints
  (generation/item/nature) validated live against pokeapi.co this session
- Pre-1.0.0 retrofit: session stamped `please_rest_version` (was missing/pre-1.0.0); Release
  `PropertyGroup` confirmed present; `.yak` packaging to be re-verified for all 3 targets
- Last active: 2026-09-18
<!-- pr:end state -->

<!-- pr:begin platform -->
## Platform context

- **Base URL:** `https://pokeapi.co/api/v2`
- **Auth method:** none — PokeAPI is fully public and unauthenticated. No Auth tab, no `Token`
  input on any component, no `ButtonComponent` (no write actions exist).
- **API version:** v2, spec dated 2022-05-23 (community OpenAPI spec at
  `oapicf/pokeapi-clients`, `specification/pokeapi.yml` on the `main` branch — the docs URL given
  at intake pointed at a nonexistent `master`/`openapi-spec/openapi.yaml` path).
- **Extra headers:** none from the API itself. The client sets a descriptive `User-Agent`
  (`PokeData-Grasshopper/1.0`) and pins TLS 1.2/1.3 explicitly — see Key design decisions.
- **Resource shape:** a flat resource graph, not a strict hierarchy. Every resource has a list
  endpoint (paged) and a detail endpoint accepting either a numeric ID or a name string in the
  same path slot. Resources cross-reference each other via `{name, url}` links rather than
  nesting.
<!-- pr:end platform -->

<!-- pr:begin structure -->
## Plugin structure

12 subcategories, 23 components. No Auth tab, no JSON Builders folder (no request bodies — every
endpoint is a plain GET), no ButtonComponent.

- **Pokemon** — Get Pokemon, Get Pokemon Batch, Get Pokemon Species, Sprite Downloader, Pokemon Cry, Batch Downloader
- **Types** — Get Type, Get Type Matrix, Type Matchup, Dual Type Matchup
- **Evolution** — Get Evolution Chain
- **Moves** — Get Move, Get Ability
- **Items** — Get Item
- **Stats** — Get Nature
- **Data** — Get Generation, Pokemon Filter
- **Geometry** — Pokemon Dimensions
- **Visualization** — Stat Radar, Stat Mesh 3D
- **Display** — Type Palette, Canvas Sprite Card
- **Presets** — Type Name (`PokemonTypePresetComponent`)
- **Utilities** — `PokeDataClient.cs`, `PluginUtilities.cs`, `PokeDataParsing.cs`

Full I/O for every component: [spec/plugin-spec.json](spec/plugin-spec.json). Human-readable
reference: [docs/index.md](docs/index.md).
<!-- pr:end structure -->

<!-- pr:begin builders -->
## JSON builders

None. PokeAPI has no write endpoints, so there are no request bodies to construct and no
`JSON Builders/` folder. `spec/plugin-spec.json`'s `builders` array is intentionally empty.
<!-- pr:end builders -->

<!-- pr:begin decisions -->
## Key design decisions

- **No auth machinery at all.** Deviates from the standard template on purpose: no Auth
  subcategory, no `Token`/`T` input anywhere, no `ButtonComponent`, no JSON builders, no payload
  contracts. Phase 9 and Gate 5 were skipped/auto-cleared.
- **Consolidated components over 1:1 endpoint mapping.** `Get Type Matrix` internally lists and
  fetches all 18 types and flattens the cross product; `Get Evolution Chain` internally resolves
  species → `evolution_chain` URL → fetches and flattens the chain. Both exist alongside their
  granular primitives (`Get Type`, `Get Pokemon Species`) rather than replacing them.
- **Pure parsing logic lives in `PokeDataParsing.cs`,** not inside the `GH_Component` classes —
  mirrors the "builders must be pure" rule from `pr-builders`, and is what lets
  `PokeData.Tests` compile the same file into a plain net8.0 test project without dragging in a
  Grasshopper/RhinoCommon reference.
- **`PokeDataClient.cs`'s static constructor sets an explicit `User-Agent` and pins TLS 1.2/1.3** —
  split `#if NET48` (classic `HttpClientHandler` + `ServicePointManager`) vs. net7.0/net7.0-windows
  (`SocketsHttpHandler` + `SslClientAuthenticationOptions`, since `ServicePointManager` is a no-op
  on the modern .NET stack and `SocketsHttpHandler` doesn't exist on .NET Framework).
- **Extend-round-1 (Batch 1) folded new fields into existing components** (`Get Pokemon` gained
  `Legacy Cry URL`/`Latest Cry URL`, `Get Type` gained `Associated Pokemon`) rather than the
  extend-loop's default append-only behavior, at the plugin author's explicit direction — new
  outputs were appended at the end of each component's output list to preserve existing output
  indices.
- **`spec/` ships inside the plugin repo,** copied here from where it was originally authored one
  directory up — see Known limitations.
- **v2.0.0 new fields all appended, never reordered** — same rule extend-round-1 established:
  `Get Move`, `Get Ability`, and `Get Type`'s new outputs are appended at the end of each output
  list so existing wire indices on canvases built against v1.0.0 keep working.
- **Localized text fields return one entry per distinct language, not per version group** —
  `PokeDataParsing.ParseLocalizedEntries` keeps only the first entry per language, since
  `flavor_text_entries` (used by `Get Move`'s new `Description` output) repeats each language once
  per game version group.
- **`Pokemon Cry`'s playback ships no in-process `.ogg` decoder.** Audio bytes are downloaded fully
  in-memory, then handed to the OS default player via a transient temp file — a one-way handoff,
  not something this plugin reads back or holds a lock on.
- **`Get Type`'s new `Color` output and `Stat Radar` are the only two v2.0.0 batch-1 additions
  with no API call at all** — `PluginUtilities.TypeColor` is a hardcoded canonical palette, and
  `Stat Radar`'s geometry is pure math (`PokeDataParsing.ComputeStatRadarPoints`, no RhinoCommon
  reference, so it stays testable in the plain net8.0 test project like the rest of
  `PokeDataParsing.cs`).
- **Batch 2 kept the pure-math/no-API pattern going**: `Pokemon Dimensions`, `Stat Mesh 3D`,
  `Type Palette`, `Canvas Sprite Card`, and `Pokemon Filter` all have `endpoint_id: null` in the
  spec and do all their work over already-fetched data or hardcoded lookups — only `Get
  Generation`, `Get Item`, `Get Nature`, and `Dual Type Matchup` (reuses `type_read`) call PokeAPI.
- **`Dual Type Matchup` multiplies two independent single-type multipliers**, not a lookup of its
  own — `PokeDataParsing.ComputeDualTypeDefense` calls the existing `MultiplierFor` once per type
  and multiplies the results, which is exactly how the real games compute dual-type damage.
- **`Item`'s `flavor_text_entries` uses a `text` field**, not `flavor_text` (Move) or `effect`
  (Ability) — confirmed against a live response this session; `Get Item`'s `Effect Text` reads
  `effect_entries`/`effect` instead, matching the Move/Ability convention, and item's `cost` field
  was observed absent from live responses entirely (parsed null-safe to `0`, not an error).
- **`Canvas Sprite Card` bakes no texture onto its mesh** — a `Bitmap` can't be painted onto mesh
  faces without a display conduit, so the card Mesh is backing geometry only; the Bitmap and a
  `Card Plane` pass through for native GH `Picture Frame` / `Text Tag 3D` to do the actual display.
- **`Stat Mesh 3D` uses a fixed-radius ring** (`ComputeRegularPolygonPoints`), unlike `Stat
  Radar`'s per-vertex radius (`ComputeStatRadarPoints`) — only the extrusion height encodes each
  stat, so the base silhouette stays a clean regular polygon at any stat distribution.
<!-- pr:end decisions -->

<!-- pr:begin limitations -->
## Known limitations

- **39 of PokeAPI's 48 resources are still not implemented.** pokemon, type, pokemon-species,
  evolution-chain, move, ability, generation, item, and nature now trace to a confirmed workflow.
  The rest (berries, contests, encounters, games, locations, machines, and the remaining
  move/pokemon taxonomy resources) follow the same uniform list+detail pattern and can be added
  via `/rest-add-feature` if a workflow needs them — see `spec/plugin-spec.json`'s `skipped_for_v1`.
- **Phase 12 Job A/B (live MCP demo build + smoke test) degraded to manual** — no Grasshopper MCP
  bridge (RhinoMCP / cordyceps / SandMartin) was available in the session that built this plugin.
  `demos/README.md` holds the manual build spec for all 5 demo canvases instead of built `.gh`
  files; `docs/examples/*.md` were written from that spec rather than from live canvases.
- **This session's sandbox could not reach `pokeapi.co`** (TLS handshake reset — see `dogfood/`
  F5), so Phase 7 (API testing) was completed via the plugin author's manual verification against
  the live API instead of the automated `pr-api-tester` run.
- **v2.0.0's Gate 6 re-test is pending on the plugin author's machine** — same no-bridge
  constraint as the original build; `demos/README.md`'s "v2.0.0 additions" section holds the
  manual check steps, now covering all 16 new/changed components across both batches.
<!-- pr:end limitations -->

<!-- pr:begin todo -->
## Todo list

See [spec/todo.md](spec/todo.md) for the full phased build checklist.
<!-- pr:end todo -->

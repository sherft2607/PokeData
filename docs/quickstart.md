# Quickstart

PokeAPI is fully public — there is no token, no sign-up, and no Auth component. Drop any
component onto the canvas and it works immediately.

1. Install PokeData via Rhino's `_PackageManager`.
2. Open Grasshopper, find the **PokeData** tab.
3. Drop **Get Pokemon** from the **Pokemon** panel onto the canvas.
4. Wire a text panel into its `Pokemon Name Or ID` input, e.g. `"pikachu"`.
5. Check the `Status` output reads `OK` — if not, read the `Response` output for the raw error.
6. Explore the other tabs: **Types** for damage matchups, **Evolution** for evolution chains,
   **Moves** for move/ability lookups.

See [workflows.md](workflows.md) for what each tab is good for.

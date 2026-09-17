# Get Evolution Chain

Resolves a species' evolution chain and flattens it into parent/child edges with trigger
conditions — no need to chain a species lookup into a separate evolution-chain lookup by hand.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Species Name Or ID | text | yes | "" | Pokemon species name or numeric ID, e.g. `"bulbasaur"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Parent Species | text (list) | Parent species name, one entry per evolution edge |
| Child Species | text (list) | Child species name, parallel to Parent Species |
| Trigger | text (list) | Evolution trigger (e.g. `level-up`, `trade`, `use-item`), parallel to Parent Species |
| Status | text | Success or error status |
| Response | text | Raw JSON response of the evolution chain call |

## API endpoint

`GET /api/v2/pokemon-species/{id}/`, then `GET` the resolved `evolution_chain.url`

## Example response (flattened, for `bulbasaur`)

```
Parent Species: [bulbasaur, ivysaur]
Child Species:  [ivysaur, venusaur]
Trigger:        [level-up, level-up]
```

## Notes

- Internally resolves the species → `evolution_chain` URL → fetches and flattens the chain in one
  component.
- Branching chains (e.g. `eevee`) produce one edge per branch, all sharing the same
  `Parent Species` entry.
- A species with no further evolutions returns empty lists, not an error.

## Related

- [Get Pokemon Species](../Pokemon/GetPokemonSpecies.md) — other species-level detail

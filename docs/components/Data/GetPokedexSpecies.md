# Get Pokedex Species

*(v4.0.0)* Looks up every species entry in a pokedex, in dex-number order, by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Pokedex Name Or ID | text | yes | "" | Pokedex name or numeric ID, e.g. `"kanto"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Species Names | text (list) | Species names in dex-entry order |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/pokedex/{id}/`

## Notes

- Reuses the same `pokedex_read` endpoint as [Get Pokedex](GetPokedex.md) — the shared
  `PokeDataClient` response cache means calling this alongside `Get Pokedex` for the same dex only
  fetches once.
- Same output shape as [Get Generation](GetGeneration.md)'s `Species Names` — feeds
  [Get Pokemon Species](../Pokemon/GetPokemonSpecies.md) or `Pokemon Filter` per entry.

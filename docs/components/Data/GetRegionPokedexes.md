# Get Region Pokedexes

*(v4.0.0)* Looks up a region's pokedex names by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Region Name Or ID | text | yes | "" | Region name or numeric ID, e.g. `"kanto"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Pokedex Name Or ID | text (list) | Names of every pokedex covering this region |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/region/{id}/`

## Notes

- Reuses the same `region_read` endpoint as [Get Region](GetRegion.md) — the shared
  `PokeDataClient` response cache means calling this alongside `Get Region` or
  [Get Region Locations](GetRegionLocations.md) for the same region only fetches once.
- `Pokedex Name Or ID` is named/typed identically to [Get Pokedex](GetPokedex.md)'s input, so it
  chains straight in with no adapter component.

# Get Region Locations

*(v4.0.0)* Looks up a region's location names by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Region Name Or ID | text | yes | "" | Region name or numeric ID, e.g. `"kanto"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Location Names | text (list) | Names of every location within this region |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/region/{id}/`

## Notes

- Reuses the same `region_read` endpoint as [Get Region](GetRegion.md) — the shared
  `PokeDataClient` response cache means calling this alongside `Get Region` or
  [Get Region Pokedexes](GetRegionPokedexes.md) for the same region only fetches once.
- Each entry can be wired into [Get Location](GetLocation.md)'s input.

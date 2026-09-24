# Get Region

*(v4.0.0)* Looks up a region's name and main generation by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Region Name Or ID | text | yes | "" | Region name or numeric ID, e.g. `"kanto"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Name | text | Region name |
| Main Generation | text | The generation this region was introduced in |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/region/{id}/`

## Notes

- Chains directly off [Get Location](GetLocation.md)'s `Region` output.
- See [Get Region Locations](GetRegionLocations.md) and [Get Region Pokedexes](GetRegionPokedexes.md)
  for the location and pokedex breakdowns, split into their own components so the lists can feed
  list-based tools without cluttering this lookup.

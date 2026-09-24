# Get Pokedex

*(v4.0.0)* Looks up a pokedex's name and whether it's a main-series dex by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Pokedex Name Or ID | text | yes | "" | Pokedex name or numeric ID, e.g. `"kanto"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Name | text | Pokedex name |
| Is Main Series | boolean | Whether this is a main-series Pokedex |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/pokedex/{id}/`

## Notes

- Chains directly off [Get Region Pokedexes](GetRegionPokedexes.md)'s output.
- See [Get Pokedex Species](GetPokedexSpecies.md) for the species roster, split into its own
  component so the list can feed list-based tools without cluttering this lookup.

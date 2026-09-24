# Get Egg Group

*(v4.0.0)* Looks up an egg group's member species by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Egg Group Name Or ID | text | yes | "" | Egg group name or numeric ID, e.g. `"monster"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Name | text | Egg group name |
| Species Names | text (list) | Every species belonging to this egg group |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/egg-group/{id}/`

## Notes

- Chains off one entry of [Get Pokemon Species](GetPokemonSpecies.md)'s `Egg Groups` list output.
- Small enough (one scalar + one list) that it isn't split into a core + breakout pair like
  Berry/Location/Region/Pokedex.

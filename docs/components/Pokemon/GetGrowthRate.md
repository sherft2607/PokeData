# Get Growth Rate

*(v4.0.0)* Looks up a growth rate's experience formula by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Growth Rate Name Or ID | text | yes | "" | Growth rate name or numeric ID, e.g. `"slow"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Formula | text | LaTeX experience formula |
| Max Level | integer | Number of levels this growth rate defines (typically 100) |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/growth-rate/{id}/`

## Notes

- Chains directly off [Get Pokemon Species](GetPokemonSpecies.md)'s `Growth Rate` output.
- See [Get Growth Rate Levels](GetGrowthRateLevels.md) for the full level-by-level experience
  curve, split into its own component so it can feed a plot directly.

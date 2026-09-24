# Get Growth Rate Levels

*(v4.0.0)* Looks up a growth rate's full level-by-level experience curve by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Growth Rate Name Or ID | text | yes | "" | Growth rate name or numeric ID, e.g. `"slow"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Levels | integer (list) | Level numbers, parallel to Experience |
| Experience | integer (list) | Total experience required per level, parallel to Levels |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/growth-rate/{id}/`

## Notes

- Reuses the same `growth-rate_read` endpoint as [Get Growth Rate](GetGrowthRate.md) — the shared
  `PokeDataClient` response cache means calling this alongside `Get Growth Rate` for the same rate
  only fetches once.

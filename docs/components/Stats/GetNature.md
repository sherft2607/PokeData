# Get Nature

*(v2.0.0)* Looks up a nature's +10%/-10% stat modifiers and berry-flavor preferences by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Nature Name Or ID | text | yes | "" | Nature name or numeric ID, e.g. `"adamant"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Increased Stat | text | Stat raised by 10% (empty for a neutral nature) |
| Decreased Stat | text | Stat lowered by 10% (empty for a neutral nature) |
| Liked Flavor | text | Berry flavor this nature likes (empty for a neutral nature) |
| Disliked Flavor | text | Berry flavor this nature dislikes (empty for a neutral nature) |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/nature/{id}/`

## Notes

- Neutral natures (e.g. `"hardy"`) have all four of `increased_stat`/`decreased_stat`/
  `likes_flavor`/`hates_flavor` as JSON `null` — those outputs come back empty rather than erroring.

# Get Berry Flavors

*(v4.0.0)* Looks up a berry's flavor potencies (spicy/dry/sweet/bitter/sour) by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Berry Name Or ID | text | yes | "" | Berry name or numeric ID, e.g. `"cheri"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Flavor Names | text (list) | Flavor names, parallel to Potencies — always all 5 flavors |
| Potencies | integer (list) | Potency per flavor (0 = not present), parallel to Flavor Names |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/berry/{id}/`

## Notes

- Reuses the same `berry_read` endpoint as [Get Berry](GetBerry.md) — split into its own
  component so the flavor list can feed list-based tools (e.g. Pokemon Filter) directly.

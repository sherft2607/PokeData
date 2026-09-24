# Get Location Areas

*(v4.0.0)* Looks up a location's sub-area names by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Location Name Or ID | text | yes | "" | Location name or numeric ID, e.g. `"canalave-city"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Area Names | text (list) | Names of every sub-area within this location |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/location/{id}/`

## Notes

- Reuses the same `location_read` endpoint as [Get Location](GetLocation.md). Only the area name
  is exposed — no separate `location-area` fetch is made per area.

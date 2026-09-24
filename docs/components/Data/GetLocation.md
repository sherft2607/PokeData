# Get Location

*(v4.0.0)* Looks up a location's name and region by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Location Name Or ID | text | yes | "" | Location name or numeric ID, e.g. `"canalave-city"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Name | text | Location name |
| Region | text | Region this location belongs to |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/location/{id}/`

## Notes

- See [Get Location Areas](GetLocationAreas.md) for the sub-area breakdown, split into its own
  component so the area list can feed list-based tools without cluttering this lookup.

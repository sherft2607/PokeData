# Get Generation

*(v2.0.0)* Looks up a generation (1-9) and returns its species roster (IDs and names) and main region.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Generation | integer | no | 1 | Generation number, 1-9 |

## Outputs

| Name | Type | Description |
|---|---|---|
| Species IDs | integer (list) | Numeric species IDs introduced in this generation |
| Species Names | text (list) | Species names introduced in this generation |
| Region | text | This generation's main region |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/generation/{id}/`

## Related

- [Pokemon Filter](PokemonFilter.md) — filter a generation's species list down to matches

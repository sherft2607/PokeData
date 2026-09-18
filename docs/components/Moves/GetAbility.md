# Get Ability

Looks up an ability's introducing generation and effect text.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Ability Name Or ID | text | yes | "" | Ability name or numeric ID, e.g. `"static"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Generation | text | Generation this ability was introduced in |
| Effect Text | text | Short English effect description |
| Status | text | Success or error status |
| Response | text | Raw JSON response |
| Short Effect | text (list) | One short effect description per available language (v2.0.0) |

## API endpoint

`GET /api/v2/ability/{id}/`

## Related

- [Get Move](GetMove.md) — same lookup pattern, for moves

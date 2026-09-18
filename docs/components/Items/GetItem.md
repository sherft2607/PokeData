# Get Item

*(v2.0.0)* Looks up an item's category, cost, fling power, effect text, and sprite URL by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Item Name Or ID | text | yes | "" | Item name or numeric ID, e.g. `"poke-ball"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Category | text | Item category |
| Cost | integer | In-game purchase cost, in Pokedollars (0 where the API omits it) |
| Fling Power | integer | Base power when used with the move Fling (0 if not flingable) |
| Effect Text | text | Short English effect description |
| Sprite URL | text | Item sprite image URL |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/item/{id}/`

## Notes

- Item's `flavor_text_entries` use a `text` field, unlike Move's `flavor_text` or Ability's
  `effect`/`short_effect` — this component reads `effect_entries`/`effect` instead, matching the
  Move/Ability pattern.

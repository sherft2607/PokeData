# Get Pokemon Batch

Looks up a list of Pokemon and returns their fields as a tree, one branch per input item.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Pokemon Names Or IDs | text (list) | yes | "" | List of Pokemon names or numeric IDs |

## Outputs

| Name | Type | Description |
|---|---|---|
| Types | text (tree) | Elemental types, per Pokemon |
| Height | number (tree) | Height in decimetres, per Pokemon |
| Weight | number (tree) | Weight in hectograms, per Pokemon |
| Base Experience | integer (tree) | Base experience yield, per Pokemon |
| Stat Names | text (tree) | Base stat names, per Pokemon |
| Stat Values | integer (tree) | Base stat values, per Pokemon |
| Sprite Front URL | text (tree) | Front-facing default sprite URL, per Pokemon |
| Sprite Artwork URL | text (tree) | Official artwork URL, per Pokemon |
| Status | text (tree) | Success or error status, per Pokemon |
| Response | text (tree) | Raw JSON response, per Pokemon |

## API endpoint

`GET /api/v2/pokemon/{id}/` — called once per list item

## Notes

- Same underlying endpoint as [Get Pokemon](GetPokemon.md), called once per input item.
- Output branches are positionally parallel to the input list (branch `i` = input item `i`).
- Blocks the Grasshopper UI thread for the duration of all calls — use with short lists (a few
  dozen items at most), not the full national dex.

## Related

- [Get Pokemon](GetPokemon.md) — the single-lookup version of this component

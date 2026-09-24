# Get Berry

*(v4.0.0)* Looks up a berry's growth time, harvest, size, and natural gift stats by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Berry Name Or ID | text | yes | "" | Berry name or numeric ID, e.g. `"cheri"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Name | text | Berry name |
| Growth Time | integer | Hours per growth stage |
| Max Harvest | integer | Maximum number harvested from one berry tree |
| Size | integer | Berry size, in millimeters |
| Smoothness | integer | Smoothness (affects Pokeblock/Poffin quality) |
| Soil Dryness | integer | Soil drying rate while planted |
| Natural Gift Power | integer | Power when used as the move Natural Gift |
| Natural Gift Type | text | Elemental type when used as Natural Gift |
| Item Name Or ID | text | The item this berry corresponds to |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/berry/{id}/`

## Notes

- Every berry is also an item — the `Item Name Or ID` output is named/typed identically to
  [Get Item](GetItem.md)'s input, so it can be wired straight in without an adapter component.
- See [Get Berry Flavors](GetBerryFlavors.md) for the flavor/potency breakdown, split into its
  own component so it can feed list-based tools without cluttering this lookup.

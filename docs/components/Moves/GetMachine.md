# Get Machine

*(v4.0.0)* Looks up which move a TM/HM machine teaches and the item it corresponds to, by numeric ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Machine ID | integer | yes | 1 | Machine's numeric ID — machines have no name field |

## Outputs

| Name | Type | Description |
|---|---|---|
| Version Group | text | Game version group this machine belongs to |
| Move Name Or ID | text | The move this machine teaches |
| Item Name Or ID | text | The TM/HM item |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/machine/{id}/`

## Notes

- `Move Name Or ID` and `Item Name Or ID` are named/typed identically to
  [Get Move](GetMove.md)'s and [Get Item](GetItem.md)'s inputs, so a machine chains straight into
  either without an adapter component.
- Unlike every other lookup component in this plugin, Machine has no name — only a numeric ID.

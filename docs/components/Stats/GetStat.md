# Get Stat

*(v4.0.0)* Looks up whether a stat only applies during battle, by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Stat Name Or ID | text | yes | "" | Stat name or numeric ID, e.g. `"attack"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Is Battle Only | boolean | Whether this stat only exists during battle (e.g. accuracy/evasion) |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/stat/{id}/`

## Notes

- Chains off [Get Nature](GetNature.md)'s `Increased Stat`/`Decreased Stat` outputs and
  `Get Pokemon`'s `Stat Names` list entries.
- See [Get Stat Affecting Natures](GetStatAffectingNatures.md) for the reverse lookup — which
  natures raise or lower a given stat.

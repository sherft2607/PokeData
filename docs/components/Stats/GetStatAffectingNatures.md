# Get Stat Affecting Natures

*(v4.0.0)* Looks up which natures increase or decrease a stat, by name or ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Stat Name Or ID | text | yes | "" | Stat name or numeric ID, e.g. `"attack"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Increasing Natures | text (list) | Natures that raise this stat by 10% |
| Decreasing Natures | text (list) | Natures that lower this stat by 10% |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/stat/{id}/`

## Notes

- Reuses the same `stat_read` endpoint as [Get Stat](GetStat.md) — the shared `PokeDataClient`
  response cache means calling this alongside `Get Stat` for the same stat only fetches once.
- The reverse lookup of [Get Nature](GetNature.md): each output entry can be wired into
  `Get Nature`'s input.

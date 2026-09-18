# Pokemon Filter

*(v2.0.0)* Filters two parallel lists (e.g. Pokemon names + a numeric value per Pokemon, such as a
stat total) by a named rule against Min/Max, returning the matches plus their original indices.
Pure list filtering — not a PokeAPI endpoint; feed it any parallel name/value lists on the canvas.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Names | text (list) | yes | — | Names, parallel to Values |
| Values | number (list) | yes | — | Numeric value per item, parallel to Names |
| Filter Rule | text | no | "Between" | One of: GreaterThan, LessThan, Between, Equals |
| Min | number | no | 0 | Lower bound (also the equality target for Equals) |
| Max | number | no | 999999 | Upper bound |

## Outputs

| Name | Type | Description |
|---|---|---|
| Filtered Names | text (list) | Names that matched the filter |
| Filtered Values | number (list) | Values that matched, parallel to Filtered Names |
| Match Indices | integer (list) | Original list index of each match |
| Status | text | Success or error status |

## Related

- [Get Generation](GetGeneration.md) — a typical source of a name list to filter

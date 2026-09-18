# Team Synergy

*(v3.0.0)* Takes up to 6 team members as parallel Names/Type 1s/Type 2s lists and computes a full
team-by-attacking-type defensive heatmap (flattened to 3 parallel lists, same convention as
[Get Type Matrix](../Types/GetTypeMatrix.md)), team coverage gaps (an attacking type every member
is weak to), and a composite vulnerability score per attacking type.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Names | text (list) | yes | — | Team member names, 1-6 entries |
| Type 1s | text (list) | yes | — | First type per member, parallel to Names |
| Type 2s | text (list) | no | — | Second type per member, parallel to Names — empty string for a single-type member |

## Outputs

| Name | Type | Description |
|---|---|---|
| Member | text (list) | Member name per heatmap row (Names.Count x 18 rows) |
| Attacking Type | text (list) | Attacking type per heatmap row, parallel to Member |
| Multiplier | number (list) | That member's defensive multiplier against that attacking type |
| Coverage Gaps | text (list) | Attacking types every team member is weak (2x+) to |
| Composite Vulnerability | number (list) | Mean defensive multiplier across the team, one per attacking type (18 items) |
| Status | text | Success or error status |

## API endpoint

`GET /api/v2/type/{id}/` (once per distinct type across the team, cached — same endpoint as
[Get Type](../Types/GetType.md))

## Related

- [Dual Type Matchup](../Types/DualTypeMatchup.md) — the same math, for one Pokemon
- [Get Type Matrix](../Types/GetTypeMatrix.md) — the flattened-matrix convention this follows

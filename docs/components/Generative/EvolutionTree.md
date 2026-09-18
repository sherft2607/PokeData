# Evolution Tree

*(v3.0.0)* Resolves a species' evolution chain (same two-call resolution as
[Get Evolution Chain](../Evolution/GetEvolutionChain.md)) and lays it out as placed tree
geometry — one point per species, one line per evolution edge — instead of just flattened edges.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Species Name Or ID | text | yes | "" | Pokemon species name or numeric ID, e.g. `"bulbasaur"` |
| Horizontal Spacing | number | no | 10.0 | Spacing between sibling nodes |
| Vertical Spacing | number | no | 10.0 | Spacing between generations (depth levels) |
| Origin | point | no | origin | Root node placement origin |

## Outputs

| Name | Type | Description |
|---|---|---|
| Node Names | text (list) | Species name per tree node |
| Node Points | point (list) | Placed point per tree node, parallel to Node Names |
| Branch Lines | line (list) | One line per evolution edge, parent node to child node |
| Triggers | text (list) | Evolution trigger per branch, parallel to Branch Lines |
| Status | text | Success or error status |
| Response | text | Raw JSON response of the evolution chain call |

## API endpoints

`GET /api/v2/pokemon-species/{id}/` then `GET` the species' `evolution_chain.url` (same as
[Get Evolution Chain](../Evolution/GetEvolutionChain.md))

## Notes

- Layout: BFS depth from the root sets each node's vertical position; a post-order pass
  centers each internal node over the average x of its children, spreading leaves left-to-right.
- Node names must be unique within one chain (true for real evolution-chain data).

## Related

- [Get Evolution Chain](../Evolution/GetEvolutionChain.md) — the flattened-edges version

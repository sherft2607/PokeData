# Get Pokemon Species

Looks up a Pokemon species' color, shape, habitat, capture rate, growth rate, egg groups, and
legendary/mythical flags.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Species Name Or ID | text | yes | "" | Pokemon species name or numeric ID, e.g. `"bulbasaur"` |

## Outputs

| Name | Type | Description |
|---|---|---|
| Color | text | Pokedex color category |
| Shape | text | Body shape category |
| Habitat | text | Habitat category (empty for species with no habitat, e.g. many legendaries) |
| Capture Rate | integer | Base capture rate (0-255, higher is easier) |
| Base Happiness | integer | Base friendship value |
| Growth Rate | text | Leveling growth rate category |
| Egg Groups | text (list) | Breeding egg groups |
| Is Legendary | boolean | True if this species is legendary |
| Is Mythical | boolean | True if this species is mythical |
| Status | text | Success or error status |
| Response | text | Raw JSON response |

## API endpoint

`GET /api/v2/pokemon-species/{id}/`

## Notes

- `Habitat` is empty for many legendary/mythical species — this is expected API behavior, not an
  error.

## Related

- [Get Pokemon](GetPokemon.md) — types/stats/sprites for the same Pokemon
- [Get Evolution Chain](../Evolution/GetEvolutionChain.md) — this species' evolution chain

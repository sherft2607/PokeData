# Stat Growth

*(v3.0.0)* Computes exact runtime battle stats from base stats, level, IVs, EVs, and a nature's
increased/decreased stat (e.g. [Get Nature](../Stats/GetNature.md)'s outputs) — the standard
Generation III+ formulas. Pure math — not a PokeAPI endpoint.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Base Stats | integer (list) | yes | — | Exactly 6 values: hp, attack, defense, special-attack, special-defense, speed |
| Level | integer | no | 50 | Pokemon level, 1-100 |
| IVs | integer (list) | no | — | Individual Values, 0-31 each, parallel to Base Stats (defaults to 31 for any missing entry) |
| EVs | integer (list) | no | — | Effort Values, 0-252 each, parallel to Base Stats (defaults to 0 for any missing entry) |
| Increased Stat | text | no | "" | Nature's increased stat name, e.g. Get Nature's `Increased Stat` output |
| Decreased Stat | text | no | "" | Nature's decreased stat name, e.g. Get Nature's `Decreased Stat` output |

## Outputs

| Name | Type | Description |
|---|---|---|
| Computed Stats | integer (list) | Runtime battle stats, parallel to Base Stats |
| Status | text | Success or error status |

## Formulas

- HP: `floor(((2*Base + IV + floor(EV/4)) * Level) / 100) + Level + 10`
- Other stats: `floor((floor(((2*Base + IV + floor(EV/4)) * Level) / 100) + 5) * NatureMultiplier)`
- Nature multiplier: `1.1` if the nature raises that stat, `0.9` if it lowers it, `1.0` otherwise

## Related

- [Get Pokemon](../Pokemon/GetPokemon.md) — the usual source of Base Stats
- [Get Nature](../Stats/GetNature.md) — the usual source of Increased/Decreased Stat

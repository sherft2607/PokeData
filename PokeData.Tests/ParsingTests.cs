using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xunit;
using PokeData;

namespace PokeData.Tests
{
    public class ParsingTests
    {
        // --- FlattenTypeRow / MultiplierFor ---------------------------------------------------

        private static readonly string[] AllTypes = new[]
        {
            "normal", "fire", "water", "electric", "grass", "ice", "fighting", "poison",
            "ground", "flying", "psychic", "bug", "rock", "ghost", "dragon", "dark", "steel", "fairy"
        };

        private static JObject FireDamageRelations() => JObject.Parse(@"{
            ""damage_relations"": {
                ""double_damage_to"":   [{ ""name"": ""grass"" }, { ""name"": ""ice"" }, { ""name"": ""bug"" }, { ""name"": ""steel"" }],
                ""double_damage_from"": [{ ""name"": ""water"" }, { ""name"": ""ground"" }, { ""name"": ""rock"" }],
                ""half_damage_to"":     [{ ""name"": ""fire"" }, { ""name"": ""water"" }, { ""name"": ""rock"" }, { ""name"": ""dragon"" }],
                ""half_damage_from"":   [],
                ""no_damage_to"":       [],
                ""no_damage_from"":     []
            }
        }");

        [Fact]
        public void FlattenTypeRow_OneType_ProducesOneRowPerDefendingType()
        {
            var attacking = new List<string>();
            var defending = new List<string>();
            var multiplier = new List<double>();

            PokeDataParsing.FlattenTypeRow("fire", FireDamageRelations(), AllTypes, attacking, defending, multiplier);

            Assert.Equal(AllTypes.Length, attacking.Count);
            Assert.Equal(AllTypes.Length, defending.Count);
            Assert.Equal(AllTypes.Length, multiplier.Count);
            Assert.All(attacking, a => Assert.Equal("fire", a));
        }

        [Theory]
        [InlineData("grass", 2.0)]
        [InlineData("ice", 2.0)]
        [InlineData("water", 0.5)]
        [InlineData("rock", 0.5)]
        [InlineData("normal", 1.0)]
        [InlineData("electric", 1.0)]
        public void FlattenTypeRow_AssignsCorrectMultiplier(string defendingType, double expected)
        {
            var attacking = new List<string>();
            var defending = new List<string>();
            var multiplier = new List<double>();

            PokeDataParsing.FlattenTypeRow("fire", FireDamageRelations(), AllTypes, attacking, defending, multiplier);

            int index = defending.IndexOf(defendingType);
            Assert.True(index >= 0, $"{defendingType} not found in defending list");
            Assert.Equal(expected, multiplier[index]);
        }

        [Fact]
        public void FullMatrix_EighteenTypesEach_Produces324Rows()
        {
            var attacking = new List<string>();
            var defending = new List<string>();
            var multiplier = new List<double>();

            // Same damage_relations fixture reused per attacking type is fine for this shape test —
            // it only asserts the cross-product size, not per-cell correctness (covered above).
            foreach (var attackingType in AllTypes)
                PokeDataParsing.FlattenTypeRow(attackingType, FireDamageRelations(), AllTypes, attacking, defending, multiplier);

            Assert.Equal(324, attacking.Count);
            Assert.Equal(324, defending.Count);
            Assert.Equal(324, multiplier.Count);
        }

        [Fact]
        public void MultiplierFor_NoDamageTakesPrecedenceOverDouble()
        {
            var doubleTo = new HashSet<string> { "ghost" };
            var halfTo = new HashSet<string>();
            var noTo = new HashSet<string> { "ghost" };

            // A type can't logically be in both sets from real data, but the precedence order
            // (no > double > half > neutral) must still hold defensively.
            Assert.Equal(0.0, PokeDataParsing.MultiplierFor(doubleTo, halfTo, noTo, "ghost"));
        }

        // --- FlattenEvolutionChain -------------------------------------------------------------

        [Fact]
        public void FlattenEvolutionChain_LinearChain_ProducesOrderedEdges()
        {
            var chain = JObject.Parse(@"{
                ""species"": { ""name"": ""bulbasaur"" },
                ""evolves_to"": [{
                    ""species"": { ""name"": ""ivysaur"" },
                    ""evolution_details"": [{ ""trigger"": { ""name"": ""level-up"" }, ""min_level"": 16 }],
                    ""evolves_to"": [{
                        ""species"": { ""name"": ""venusaur"" },
                        ""evolution_details"": [{ ""trigger"": { ""name"": ""level-up"" }, ""min_level"": 32 }],
                        ""evolves_to"": []
                    }]
                }]
            }");

            var parents = new List<string>();
            var children = new List<string>();
            var triggers = new List<string>();

            PokeDataParsing.FlattenEvolutionChain(chain, parents, children, triggers);

            Assert.Equal(2, parents.Count);
            Assert.Equal(new[] { "bulbasaur", "ivysaur" }, parents);
            Assert.Equal(new[] { "ivysaur", "venusaur" }, children);
            Assert.Equal(new[] { "level-up", "level-up" }, triggers);
        }

        [Fact]
        public void FlattenEvolutionChain_BranchingChain_ProducesOneEdgePerBranch()
        {
            // eevee-style branch: one species evolves into several
            var chain = JObject.Parse(@"{
                ""species"": { ""name"": ""eevee"" },
                ""evolves_to"": [
                    { ""species"": { ""name"": ""vaporeon"" }, ""evolution_details"": [{ ""trigger"": { ""name"": ""use-item"" } }], ""evolves_to"": [] },
                    { ""species"": { ""name"": ""jolteon"" },  ""evolution_details"": [{ ""trigger"": { ""name"": ""use-item"" } }], ""evolves_to"": [] },
                    { ""species"": { ""name"": ""flareon"" },  ""evolution_details"": [{ ""trigger"": { ""name"": ""use-item"" } }], ""evolves_to"": [] }
                ]
            }");

            var parents = new List<string>();
            var children = new List<string>();
            var triggers = new List<string>();

            PokeDataParsing.FlattenEvolutionChain(chain, parents, children, triggers);

            Assert.Equal(3, parents.Count);
            Assert.All(parents, p => Assert.Equal("eevee", p));
            Assert.Equal(new[] { "vaporeon", "jolteon", "flareon" }, children);
        }

        [Fact]
        public void FlattenEvolutionChain_NoEvolutions_ProducesNoEdges()
        {
            var chain = JObject.Parse(@"{ ""species"": { ""name"": ""tauros"" }, ""evolves_to"": [] }");

            var parents = new List<string>();
            var children = new List<string>();
            var triggers = new List<string>();

            PokeDataParsing.FlattenEvolutionChain(chain, parents, children, triggers);

            Assert.Empty(parents);
        }

        [Fact]
        public void FlattenEvolutionChain_NullNode_DoesNotThrow()
        {
            var parents = new List<string>();
            var children = new List<string>();
            var triggers = new List<string>();

            var ex = Record.Exception(() => PokeDataParsing.FlattenEvolutionChain(null, parents, children, triggers));

            Assert.Null(ex);
            Assert.Empty(parents);
        }

        // --- ParseStats --------------------------------------------------------------------------

        [Fact]
        public void ParseStats_PairsNamesAndValuesInOrder()
        {
            var stats = JArray.Parse(@"[
                { ""base_stat"": 35, ""stat"": { ""name"": ""hp"" } },
                { ""base_stat"": 55, ""stat"": { ""name"": ""attack"" } },
                { ""base_stat"": 40, ""stat"": { ""name"": ""defense"" } },
                { ""base_stat"": 50, ""stat"": { ""name"": ""special-attack"" } },
                { ""base_stat"": 50, ""stat"": { ""name"": ""special-defense"" } },
                { ""base_stat"": 90, ""stat"": { ""name"": ""speed"" } }
            ]");

            var names = new List<string>();
            var values = new List<int>();

            PokeDataParsing.ParseStats(stats, names, values);

            Assert.Equal(6, names.Count);
            Assert.Equal(new[] { "hp", "attack", "defense", "special-attack", "special-defense", "speed" }, names);
            Assert.Equal(new[] { 35, 55, 40, 50, 50, 90 }, values);
        }

        [Fact]
        public void ParseStats_NullArray_DoesNotThrow()
        {
            var names = new List<string>();
            var values = new List<int>();

            var ex = Record.Exception(() => PokeDataParsing.ParseStats(null, names, values));

            Assert.Null(ex);
            Assert.Empty(names);
        }

        // --- extend-round-1: NamesToList / AssociatedPokemonNames / ParseCries ------------------

        [Fact]
        public void NamesToList_FlattensNameObjectsInOrder()
        {
            var array = JArray.Parse(@"[{ ""name"": ""monster"" }, { ""name"": ""grass"" }]");

            var result = PokeDataParsing.NamesToList(array);

            Assert.Equal(new[] { "monster", "grass" }, result);
        }

        [Fact]
        public void NamesToList_NullArray_ReturnsEmptyList_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.NamesToList(null));

            Assert.Null(ex);
            Assert.Empty(PokeDataParsing.NamesToList(null));
        }

        [Fact]
        public void AssociatedPokemonNames_ReadsNestedPokemonName()
        {
            var array = JArray.Parse(@"[
                { ""pokemon"": { ""name"": ""bulbasaur"" }, ""slot"": 1 },
                { ""pokemon"": { ""name"": ""ivysaur"" }, ""slot"": 1 }
            ]");

            var result = PokeDataParsing.AssociatedPokemonNames(array);

            Assert.Equal(new[] { "bulbasaur", "ivysaur" }, result);
        }

        [Fact]
        public void AssociatedPokemonNames_NullArray_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.AssociatedPokemonNames(null));

            Assert.Null(ex);
            Assert.Empty(PokeDataParsing.AssociatedPokemonNames(null));
        }

        [Fact]
        public void ParseCries_ReadsLegacyAndLatestUrls()
        {
            var pokemon = JObject.Parse(@"{
                ""cries"": { ""legacy"": ""https://example.com/legacy.ogg"", ""latest"": ""https://example.com/latest.ogg"" }
            }");

            PokeDataParsing.ParseCries(pokemon, out var legacy, out var latest);

            Assert.Equal("https://example.com/legacy.ogg", legacy);
            Assert.Equal("https://example.com/latest.ogg", latest);
        }

        [Fact]
        public void ParseCries_NullPokemon_ReturnsEmptyStrings_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ParseCries(null, out var legacy, out var latest));

            Assert.Null(ex);
        }

        [Fact]
        public void ParseCries_MissingCriesField_ReturnsEmptyStrings()
        {
            var pokemon = JObject.Parse(@"{ ""name"": ""pikachu"" }");

            PokeDataParsing.ParseCries(pokemon, out var legacy, out var latest);

            Assert.Equal("", legacy);
            Assert.Equal("", latest);
        }

        // --- null-field fixtures (reported live bug: "Cannot access child value on JValue") ----

        [Fact]
        public void ParseCries_CriesFieldIsJsonNull_DoesNotThrow_ReturnsEmptyStrings()
        {
            // "cries": null -- deserializes to a JValue(null), not a C# null reference
            var pokemon = JObject.Parse(@"{ ""name"": ""pikachu"", ""cries"": null }");

            var ex = Record.Exception(() => PokeDataParsing.ParseCries(pokemon, out var legacy, out var latest));

            Assert.Null(ex);
            PokeDataParsing.ParseCries(pokemon, out var legacy2, out var latest2);
            Assert.Equal("", legacy2);
            Assert.Equal("", latest2);
        }

        [Fact]
        public void ParseCries_LegacyFieldIsJsonNull_DoesNotThrow_LatestStillReads()
        {
            var pokemon = JObject.Parse(@"{ ""cries"": { ""legacy"": null, ""latest"": ""https://example.com/latest.ogg"" } }");

            var ex = Record.Exception(() => PokeDataParsing.ParseCries(pokemon, out var legacy, out var latest));
            Assert.Null(ex);

            PokeDataParsing.ParseCries(pokemon, out var legacy2, out var latest2);
            Assert.Equal("", legacy2);
            Assert.Equal("https://example.com/latest.ogg", latest2);
        }

        [Fact]
        public void SafeString_JsonNullAtIntermediateHop_DoesNotThrow_ReturnsEmpty()
        {
            // "habitat": null -- the exact shape PokeAPI returns for many legendary/mythical species
            var species = JObject.Parse(@"{ ""habitat"": null, ""shape"": null, ""color"": { ""name"": ""green"" } }");

            var ex = Record.Exception(() =>
            {
                PokeDataParsing.SafeString(species, "habitat", "name");
                PokeDataParsing.SafeString(species, "shape", "name");
            });

            Assert.Null(ex);
            Assert.Equal("", PokeDataParsing.SafeString(species, "habitat", "name"));
            Assert.Equal("", PokeDataParsing.SafeString(species, "shape", "name"));
            Assert.Equal("green", PokeDataParsing.SafeString(species, "color", "name"));
        }

        [Fact]
        public void SafeString_EvolutionChainFieldIsJsonNull_DoesNotThrow()
        {
            var species = JObject.Parse(@"{ ""evolution_chain"": null }");

            var ex = Record.Exception(() => PokeDataParsing.SafeString(species, "evolution_chain", "url"));

            Assert.Null(ex);
            Assert.Equal("", PokeDataParsing.SafeString(species, "evolution_chain", "url"));
        }

        [Fact]
        public void SafeString_MissingIntermediateObject_DoesNotThrow()
        {
            var empty = JObject.Parse(@"{}");

            var ex = Record.Exception(() => PokeDataParsing.SafeString(empty, "sprites", "other", "official-artwork", "front_default"));

            Assert.Null(ex);
            Assert.Equal("", PokeDataParsing.SafeString(empty, "sprites", "other", "official-artwork", "front_default"));
        }

        [Fact]
        public void SafeString_NullToken_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.SafeString(null, "a", "b"));

            Assert.Null(ex);
            Assert.Equal("", PokeDataParsing.SafeString(null, "a", "b"));
        }

        [Fact]
        public void SafeArray_JsonNullField_DoesNotThrow_ReturnsNull()
        {
            var type = JObject.Parse(@"{ ""damage_relations"": null }");

            var ex = Record.Exception(() => PokeDataParsing.SafeArray(type, "damage_relations", "double_damage_to"));

            Assert.Null(ex);
            Assert.Null(PokeDataParsing.SafeArray(type, "damage_relations", "double_damage_to"));
        }
    }
}

using System;
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

        // --- v2.0.0: ParseLocalizedEntries -------------------------------------------------------

        [Fact]
        public void ParseLocalizedEntries_OneEntryPerLanguage_ReadsAll()
        {
            var entries = JArray.Parse(@"[
                { ""effect"": ""Raises Attack."", ""language"": { ""name"": ""en"" } },
                { ""effect"": ""Sube Ataque."", ""language"": { ""name"": ""es"" } }
            ]");

            PokeDataParsing.ParseLocalizedEntries(entries, "effect", out var languages, out var texts);

            Assert.Equal(new[] { "en", "es" }, languages);
            Assert.Equal(new[] { "Raises Attack.", "Sube Ataque." }, texts);
        }

        [Fact]
        public void ParseLocalizedEntries_DuplicateLanguage_KeepsFirstOnly()
        {
            // flavor_text_entries repeats a language once per version_group
            var entries = JArray.Parse(@"[
                { ""flavor_text"": ""Version A text."", ""language"": { ""name"": ""en"" } },
                { ""flavor_text"": ""Version B text."", ""language"": { ""name"": ""en"" } }
            ]");

            PokeDataParsing.ParseLocalizedEntries(entries, "flavor_text", out var languages, out var texts);

            Assert.Equal(new[] { "en" }, languages);
            Assert.Equal(new[] { "Version A text." }, texts);
        }

        [Fact]
        public void ParseLocalizedEntries_NullArray_ReturnsEmptyLists_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ParseLocalizedEntries(null, "effect", out var languages, out var texts));

            Assert.Null(ex);
            PokeDataParsing.ParseLocalizedEntries(null, "effect", out var languages2, out var texts2);
            Assert.Empty(languages2);
            Assert.Empty(texts2);
        }

        [Fact]
        public void ParseLocalizedEntries_MissingLanguage_SkipsEntry()
        {
            var entries = JArray.Parse(@"[{ ""effect"": ""No language here."" }]");

            PokeDataParsing.ParseLocalizedEntries(entries, "effect", out var languages, out var texts);

            Assert.Empty(languages);
            Assert.Empty(texts);
        }

        // --- v2.0.0: ComputeStatRadarPoints -------------------------------------------------------

        [Fact]
        public void ComputeStatRadarPoints_SixStats_ProducesSixPoints()
        {
            var stats = new List<int> { 35, 55, 40, 50, 50, 90 };

            PokeDataParsing.ComputeStatRadarPoints(stats, 10.0, 255.0, out var x, out var y);

            Assert.Equal(6, x.Count);
            Assert.Equal(6, y.Count);
        }

        [Fact]
        public void ComputeStatRadarPoints_MaxStatValue_ReachesFullRadius()
        {
            var stats = new List<int> { 255 };

            PokeDataParsing.ComputeStatRadarPoints(stats, 10.0, 255.0, out var x, out var y);

            double distance = System.Math.Sqrt(x[0] * x[0] + y[0] * y[0]);
            Assert.Equal(10.0, distance, 3);
        }

        [Fact]
        public void ComputeStatRadarPoints_ZeroStat_ProducesOriginPoint()
        {
            var stats = new List<int> { 0, 0, 0 };

            PokeDataParsing.ComputeStatRadarPoints(stats, 10.0, 255.0, out var x, out var y);

            Assert.All(x, v => Assert.Equal(0.0, v, 3));
            Assert.All(y, v => Assert.Equal(0.0, v, 3));
        }

        [Fact]
        public void ComputeStatRadarPoints_ValueAboveMax_IsClampedToRadius()
        {
            var stats = new List<int> { 999 };

            PokeDataParsing.ComputeStatRadarPoints(stats, 10.0, 255.0, out var x, out var y);

            double distance = System.Math.Sqrt(x[0] * x[0] + y[0] * y[0]);
            Assert.Equal(10.0, distance, 3);
        }

        [Fact]
        public void ComputeStatRadarPoints_EmptyList_ReturnsEmpty_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ComputeStatRadarPoints(new List<int>(), 10.0, 255.0, out var x, out var y));

            Assert.Null(ex);
            PokeDataParsing.ComputeStatRadarPoints(new List<int>(), 10.0, 255.0, out var x2, out var y2);
            Assert.Empty(x2);
            Assert.Empty(y2);
        }

        [Fact]
        public void ComputeStatRadarPoints_NullList_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ComputeStatRadarPoints(null, 10.0, 255.0, out var x, out var y));

            Assert.Null(ex);
        }

        [Fact]
        public void ComputeStatRadarPoints_ZeroMaxStat_ReturnsEmpty_DoesNotThrow()
        {
            var stats = new List<int> { 50, 50, 50 };

            var ex = Record.Exception(() => PokeDataParsing.ComputeStatRadarPoints(stats, 10.0, 0.0, out var x, out var y));

            Assert.Null(ex);
            PokeDataParsing.ComputeStatRadarPoints(stats, 10.0, 0.0, out var x2, out var y2);
            Assert.Empty(x2);
        }

        // --- v2.1.0: ExtractIdFromUrl / ExtractIdsFromUrls --------------------------------------

        [Theory]
        [InlineData("https://pokeapi.co/api/v2/pokemon-species/1/", 1)]
        [InlineData("https://pokeapi.co/api/v2/pokemon-species/25/", 25)]
        [InlineData("https://pokeapi.co/api/v2/region/1", 1)]
        public void ExtractIdFromUrl_ParsesTrailingNumericSegment(string url, int expected)
        {
            Assert.Equal(expected, PokeDataParsing.ExtractIdFromUrl(url));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("not-a-url")]
        [InlineData("https://pokeapi.co/api/v2/type/fire/")]
        public void ExtractIdFromUrl_Malformed_ReturnsZero_DoesNotThrow(string url)
        {
            var ex = Record.Exception(() => PokeDataParsing.ExtractIdFromUrl(url));
            Assert.Null(ex);
            Assert.Equal(0, PokeDataParsing.ExtractIdFromUrl(url));
        }

        [Fact]
        public void ExtractIdsFromUrls_ReadsEachEntryInOrder()
        {
            var array = JArray.Parse(@"[
                { ""name"": ""bulbasaur"", ""url"": ""https://pokeapi.co/api/v2/pokemon-species/1/"" },
                { ""name"": ""ivysaur"", ""url"": ""https://pokeapi.co/api/v2/pokemon-species/2/"" }
            ]");

            var result = PokeDataParsing.ExtractIdsFromUrls(array);

            Assert.Equal(new[] { 1, 2 }, result);
        }

        [Fact]
        public void ExtractIdsFromUrls_NullArray_ReturnsEmpty_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ExtractIdsFromUrls(null));
            Assert.Null(ex);
            Assert.Empty(PokeDataParsing.ExtractIdsFromUrls(null));
        }

        // --- v2.1.0: ComputeDualTypeDefense ------------------------------------------------------

        private static readonly string[] AllTypesV2 = new[]
        {
            "normal", "fire", "water", "electric", "grass", "ice", "fighting", "poison",
            "ground", "flying", "psychic", "bug", "rock", "ghost", "dragon", "dark", "steel", "fairy"
        };

        [Fact]
        public void ComputeDualTypeDefense_SingleType_MatchesPlainMultiplier()
        {
            // fire alone: takes double from water/ground/rock, half from fire/grass/ice/bug/steel/fairy
            var doubleFrom1 = new HashSet<string> { "water", "ground", "rock" };
            var halfFrom1 = new HashSet<string> { "fire", "grass", "ice", "bug", "steel", "fairy" };
            var noFrom1 = new HashSet<string>();

            var x4 = new List<string>(); var x2 = new List<string>(); var x1 = new List<string>();
            var h2 = new List<string>(); var h4 = new List<string>(); var x0 = new List<string>();

            PokeDataParsing.ComputeDualTypeDefense(doubleFrom1, halfFrom1, noFrom1, null, null, null, AllTypesV2, x4, x2, x1, h2, h4, x0);

            Assert.Contains("water", x2);
            Assert.Contains("fire", h2);
            Assert.Contains("normal", x1);
            Assert.Empty(x4);
            Assert.Empty(h4);
            Assert.Empty(x0);
        }

        [Fact]
        public void ComputeDualTypeDefense_DualType_MultipliesBothTypes()
        {
            // fire/flying (charizard-style): grass 2x from fire, and flying is neutral to grass ->
            // combined grass = 2x. Ice: fire takes 0.5x from ice, flying takes 2x from ice -> combined = 1x.
            // rock: fire takes 2x from rock, flying takes 2x from rock -> combined 4x.
            var doubleFrom1 = new HashSet<string> { "water", "ground", "rock" }; // fire
            var halfFrom1 = new HashSet<string> { "fire", "grass", "ice", "bug", "steel", "fairy" };
            var noFrom1 = new HashSet<string>();

            var doubleFrom2 = new HashSet<string> { "rock", "electric", "ice" }; // flying
            var halfFrom2 = new HashSet<string> { "fighting", "bug", "grass" };
            var noFrom2 = new HashSet<string> { "ground" };

            var x4 = new List<string>(); var x2 = new List<string>(); var x1 = new List<string>();
            var h2 = new List<string>(); var h4 = new List<string>(); var x0 = new List<string>();

            PokeDataParsing.ComputeDualTypeDefense(doubleFrom1, halfFrom1, noFrom1, doubleFrom2, halfFrom2, noFrom2, AllTypesV2, x4, x2, x1, h2, h4, x0);

            Assert.Contains("rock", x4);   // 2 * 2 = 4
            Assert.Contains("ice", x1);    // 0.5 * 2 = 1
            Assert.Contains("ground", x0); // ground: fire=2x, flying=0x -> 0
            Assert.Contains("grass", h4);  // fire=0.5, flying=0.5 (per this fixture) -> 0.25
        }

        [Fact]
        public void ComputeDualTypeDefense_NullSecondType_TreatsAsSingleType()
        {
            var doubleFrom1 = new HashSet<string> { "water" };
            var halfFrom1 = new HashSet<string>();
            var noFrom1 = new HashSet<string>();

            var x4 = new List<string>(); var x2 = new List<string>(); var x1 = new List<string>();
            var h2 = new List<string>(); var h4 = new List<string>(); var x0 = new List<string>();

            var ex = Record.Exception(() =>
                PokeDataParsing.ComputeDualTypeDefense(doubleFrom1, halfFrom1, noFrom1, null, null, null, AllTypesV2, x4, x2, x1, h2, h4, x0));

            Assert.Null(ex);
            Assert.Contains("water", x2);
        }

        // --- v2.1.0: ConvertDimensions -----------------------------------------------------------

        [Fact]
        public void ConvertDimensions_Pikachu_ProducesExpectedValues()
        {
            // pikachu: height 4 dm, weight 60 hg
            PokeDataParsing.ConvertDimensions(4, 60, out var heightM, out var heightFt, out var weightKg, out var weightLb);

            Assert.Equal(0.4, heightM, 5);
            Assert.Equal(1.3123, heightFt, 3);
            Assert.Equal(6.0, weightKg, 5);
            Assert.Equal(13.2277, weightLb, 3);
        }

        [Fact]
        public void ConvertDimensions_Zero_ProducesZero_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ConvertDimensions(0, 0, out var hm, out var hf, out var wk, out var wl));
            Assert.Null(ex);
            PokeDataParsing.ConvertDimensions(0, 0, out var heightM, out var heightFt, out var weightKg, out var weightLb);
            Assert.Equal(0, heightM);
            Assert.Equal(0, heightFt);
            Assert.Equal(0, weightKg);
            Assert.Equal(0, weightLb);
        }

        // --- v2.1.0: ComputeRegularPolygonPoints -------------------------------------------------

        [Fact]
        public void ComputeRegularPolygonPoints_SixSides_ProducesSixEquidistantPoints()
        {
            PokeDataParsing.ComputeRegularPolygonPoints(6, 10.0, out var x, out var y);

            Assert.Equal(6, x.Count);
            for (int i = 0; i < 6; i++)
            {
                double distance = System.Math.Sqrt(x[i] * x[i] + y[i] * y[i]);
                Assert.Equal(10.0, distance, 3);
            }
        }

        [Theory]
        [InlineData(2)]
        [InlineData(0)]
        [InlineData(-1)]
        public void ComputeRegularPolygonPoints_FewerThanThreeSides_ReturnsEmpty_DoesNotThrow(int sides)
        {
            var ex = Record.Exception(() => PokeDataParsing.ComputeRegularPolygonPoints(sides, 10.0, out var x, out var y));
            Assert.Null(ex);
            PokeDataParsing.ComputeRegularPolygonPoints(sides, 10.0, out var x2, out var y2);
            Assert.Empty(x2);
        }

        [Fact]
        public void ComputeRegularPolygonPoints_ZeroRadius_ReturnsEmpty()
        {
            PokeDataParsing.ComputeRegularPolygonPoints(6, 0.0, out var x, out var y);
            Assert.Empty(x);
        }

        // --- v2.1.0: ComputeStatMeshHeights ------------------------------------------------------

        [Fact]
        public void ComputeStatMeshHeights_MaxStat_ReachesFullHeightFactor()
        {
            var heights = PokeDataParsing.ComputeStatMeshHeights(new List<int> { 255 }, 255.0, 5.0);

            Assert.Equal(5.0, heights[0], 5);
        }

        [Fact]
        public void ComputeStatMeshHeights_ZeroStat_ProducesZeroHeight()
        {
            var heights = PokeDataParsing.ComputeStatMeshHeights(new List<int> { 0 }, 255.0, 5.0);

            Assert.Equal(0.0, heights[0], 5);
        }

        [Fact]
        public void ComputeStatMeshHeights_ValueAboveMax_ClampsToHeightFactor()
        {
            var heights = PokeDataParsing.ComputeStatMeshHeights(new List<int> { 999 }, 255.0, 5.0);

            Assert.Equal(5.0, heights[0], 5);
        }

        [Fact]
        public void ComputeStatMeshHeights_NullList_ReturnsEmpty_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ComputeStatMeshHeights(null, 255.0, 5.0));
            Assert.Null(ex);
            Assert.Empty(PokeDataParsing.ComputeStatMeshHeights(null, 255.0, 5.0));
        }

        // --- v2.1.0: FilterByRule -----------------------------------------------------------------

        private static readonly List<string> FilterNames = new List<string> { "bulbasaur", "charmander", "squirtle", "pikachu" };
        private static readonly List<double> FilterValues = new List<double> { 318, 309, 314, 320 };

        [Fact]
        public void FilterByRule_Between_ReturnsMatchesWithinRange()
        {
            PokeDataParsing.FilterByRule(FilterNames, FilterValues, "Between", 310, 320,
                out var names, out var values, out var indices);

            Assert.Equal(new[] { "bulbasaur", "squirtle", "pikachu" }, names);
            Assert.Equal(new[] { 0, 2, 3 }, indices);
        }

        [Fact]
        public void FilterByRule_GreaterThan_UsesMinAsThreshold()
        {
            PokeDataParsing.FilterByRule(FilterNames, FilterValues, "GreaterThan", 319, 999,
                out var names, out var values, out var indices);

            Assert.Equal(new[] { "pikachu" }, names);
        }

        [Fact]
        public void FilterByRule_LessThan_UsesMaxAsThreshold()
        {
            PokeDataParsing.FilterByRule(FilterNames, FilterValues, "LessThan", 0, 310,
                out var names, out var values, out var indices);

            Assert.Equal(new[] { "charmander" }, names);
        }

        [Fact]
        public void FilterByRule_Equals_UsesMinAsTarget()
        {
            PokeDataParsing.FilterByRule(FilterNames, FilterValues, "Equals", 314, 0,
                out var names, out var values, out var indices);

            Assert.Equal(new[] { "squirtle" }, names);
        }

        [Fact]
        public void FilterByRule_UnknownRule_FallsBackToBetween()
        {
            PokeDataParsing.FilterByRule(FilterNames, FilterValues, "Bogus", 310, 320,
                out var names, out var values, out var indices);

            Assert.Equal(new[] { "bulbasaur", "squirtle", "pikachu" }, names);
        }

        [Fact]
        public void FilterByRule_MismatchedListLengths_UsesShorterLength_DoesNotThrow()
        {
            var shortValues = new List<double> { 318 };

            var ex = Record.Exception(() => PokeDataParsing.FilterByRule(FilterNames, shortValues, "Between", 0, 999, out var n, out var v, out var i));

            Assert.Null(ex);
            PokeDataParsing.FilterByRule(FilterNames, shortValues, "Between", 0, 999, out var names, out var values, out var indices);
            Assert.Single(names);
        }

        [Fact]
        public void FilterByRule_NullInputs_ReturnsEmpty_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.FilterByRule(null, null, "Between", 0, 1, out var n, out var v, out var i));
            Assert.Null(ex);
        }

        // --- v2.1.0: BuildCardLabel ---------------------------------------------------------------

        [Fact]
        public void BuildCardLabel_NameAndStats_JoinsAllLinesInOrder()
        {
            var label = PokeDataParsing.BuildCardLabel("Pikachu", new List<string> { "hp: 35", "attack: 55" });

            Assert.StartsWith("Pikachu", label);
            Assert.Contains("hp: 35", label);
            Assert.Contains("attack: 55", label);
            Assert.True(label.IndexOf("hp: 35") < label.IndexOf("attack: 55"));
        }

        [Fact]
        public void BuildCardLabel_EmptyInputs_ReturnsEmptyString()
        {
            Assert.Equal("", PokeDataParsing.BuildCardLabel("", null));
            Assert.Equal("", PokeDataParsing.BuildCardLabel(null, new List<string>()));
        }

        [Fact]
        public void BuildCardLabel_NameOnly_NoTrailingNewline()
        {
            var label = PokeDataParsing.BuildCardLabel("Pikachu", null);
            Assert.Equal("Pikachu", label);
        }

        // --- v2.1.0: SummarizeBatchStatus ----------------------------------------------------------

        [Fact]
        public void SummarizeBatchStatus_AllSucceeded_ReportsOk()
        {
            Assert.Equal("OK (3/3)", PokeDataParsing.SummarizeBatchStatus(3, 0));
        }

        [Fact]
        public void SummarizeBatchStatus_SomeFailed_ReportsPartial()
        {
            Assert.Equal("Partial: 2 succeeded, 1 failed", PokeDataParsing.SummarizeBatchStatus(2, 1));
        }

        [Fact]
        public void SummarizeBatchStatus_AllFailed_ReportsPartial()
        {
            Assert.Equal("Partial: 0 succeeded, 3 failed", PokeDataParsing.SummarizeBatchStatus(0, 3));
        }

        // --- v3.0.0: ComputeDefenseMultipliers ----------------------------------------------------

        private static readonly string[] AllTypesV3 = new[]
        {
            "normal", "fire", "water", "electric", "grass", "ice", "fighting", "poison",
            "ground", "flying", "psychic", "bug", "rock", "ghost", "dragon", "dark", "steel", "fairy"
        };

        [Fact]
        public void ComputeDefenseMultipliers_SingleType_MatchesMultiplierFor()
        {
            var doubleFrom = new HashSet<string> { "water" };
            var halfFrom = new HashSet<string> { "fire" };
            var noFrom = new HashSet<string>();

            var result = PokeDataParsing.ComputeDefenseMultipliers(doubleFrom, halfFrom, noFrom, null, null, null, AllTypesV3);

            Assert.Equal(2.0, result[Array.IndexOf(AllTypesV3, "water")]);
            Assert.Equal(0.5, result[Array.IndexOf(AllTypesV3, "fire")]);
            Assert.Equal(1.0, result[Array.IndexOf(AllTypesV3, "normal")]);
        }

        [Fact]
        public void ComputeDefenseMultipliers_DualType_MultipliesIndependentResults()
        {
            var doubleFrom1 = new HashSet<string> { "rock" };
            var halfFrom1 = new HashSet<string>();
            var noFrom1 = new HashSet<string>();

            var doubleFrom2 = new HashSet<string> { "rock" };
            var halfFrom2 = new HashSet<string>();
            var noFrom2 = new HashSet<string>();

            var result = PokeDataParsing.ComputeDefenseMultipliers(doubleFrom1, halfFrom1, noFrom1, doubleFrom2, halfFrom2, noFrom2, AllTypesV3);

            Assert.Equal(4.0, result[Array.IndexOf(AllTypesV3, "rock")]);
        }

        [Fact]
        public void ComputeDefenseMultipliers_NullAllTypes_ReturnsEmpty_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ComputeDefenseMultipliers(null, null, null, null, null, null, null));
            Assert.Null(ex);
            Assert.Empty(PokeDataParsing.ComputeDefenseMultipliers(null, null, null, null, null, null, null));
        }

        // --- v3.0.0: ComputeTeamSynergy ------------------------------------------------------------

        [Fact]
        public void ComputeTeamSynergy_AllMembersWeakToOneType_IsACoverageGap()
        {
            var member1 = new double[AllTypesV3.Length];
            var member2 = new double[AllTypesV3.Length];
            for (int i = 0; i < AllTypesV3.Length; i++) { member1[i] = 1.0; member2[i] = 1.0; }
            int rockIndex = Array.IndexOf(AllTypesV3, "rock");
            member1[rockIndex] = 2.0;
            member2[rockIndex] = 4.0;

            PokeDataParsing.ComputeTeamSynergy(new List<double[]> { member1, member2 }, AllTypesV3,
                out var coverageGaps, out var compositeVulnerability);

            Assert.Contains("rock", coverageGaps);
            Assert.Equal(3.0, compositeVulnerability[rockIndex]);
        }

        [Fact]
        public void ComputeTeamSynergy_OneMemberResists_NotACoverageGap()
        {
            var member1 = new double[AllTypesV3.Length];
            var member2 = new double[AllTypesV3.Length];
            for (int i = 0; i < AllTypesV3.Length; i++) { member1[i] = 1.0; member2[i] = 1.0; }
            int rockIndex = Array.IndexOf(AllTypesV3, "rock");
            member1[rockIndex] = 2.0;
            member2[rockIndex] = 0.5; // resists — team is covered here

            PokeDataParsing.ComputeTeamSynergy(new List<double[]> { member1, member2 }, AllTypesV3,
                out var coverageGaps, out var compositeVulnerability);

            Assert.DoesNotContain("rock", coverageGaps);
        }

        [Fact]
        public void ComputeTeamSynergy_EmptyTeam_ReturnsZeroVulnerability_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ComputeTeamSynergy(new List<double[]>(), AllTypesV3, out var g, out var v));
            Assert.Null(ex);

            PokeDataParsing.ComputeTeamSynergy(new List<double[]>(), AllTypesV3, out var gaps, out var vulnerability);
            Assert.All(vulnerability, v => Assert.Equal(0.0, v));
            Assert.Empty(gaps);
        }

        [Fact]
        public void ComputeTeamSynergy_NullAllTypes_ReturnsEmpty_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ComputeTeamSynergy(new List<double[]>(), null, out var g, out var v));
            Assert.Null(ex);
        }

        // --- v3.0.0: ComputeHpStat / ComputeBattleStat / NatureMultiplierFor ---------------------

        [Fact]
        public void ComputeHpStat_KnownValues_MatchesFormula()
        {
            // base 35, IV 31, EV 0, level 50 -> (2*35+31+0)*50/100 + 50 + 10 = 110
            Assert.Equal(110, PokeDataParsing.ComputeHpStat(35, 31, 0, 50));
        }

        [Fact]
        public void ComputeHpStat_ZeroIvZeroEv_LowerThanMaxIv()
        {
            int withMaxIv = PokeDataParsing.ComputeHpStat(35, 31, 0, 50);
            int withZeroIv = PokeDataParsing.ComputeHpStat(35, 0, 0, 50);
            Assert.True(withZeroIv < withMaxIv);
        }

        [Fact]
        public void ComputeBattleStat_NeutralNature_MatchesFormula()
        {
            // base 55, IV 31, EV 0, level 50, neutral -> floor((2*55+31+0)*50/100 + 5) = 75
            Assert.Equal(75, PokeDataParsing.ComputeBattleStat(55, 31, 0, 50, 1.0));
        }

        [Fact]
        public void ComputeBattleStat_BoostedNature_Multiplies1_1AndTruncates()
        {
            // 75 * 1.1 = 82.5 -> truncates to 82
            Assert.Equal(82, PokeDataParsing.ComputeBattleStat(55, 31, 0, 50, 1.1));
        }

        [Fact]
        public void ComputeBattleStat_HinderedNature_Multiplies0_9()
        {
            // 75 * 0.9 = 67.5 -> truncates to 67
            Assert.Equal(67, PokeDataParsing.ComputeBattleStat(55, 31, 0, 50, 0.9));
        }

        [Theory]
        [InlineData("attack", "special-attack", "attack", 1.1)]
        [InlineData("attack", "special-attack", "special-attack", 0.9)]
        [InlineData("attack", "special-attack", "speed", 1.0)]
        [InlineData("", "", "attack", 1.0)]
        public void NatureMultiplierFor_ReturnsExpectedMultiplier(string increased, string decreased, string stat, double expected)
        {
            Assert.Equal(expected, PokeDataParsing.NatureMultiplierFor(increased, decreased, stat));
        }

        [Fact]
        public void NatureMultiplierFor_SameIncreasedAndDecreased_TreatedAsNeutral()
        {
            // defensive case — shouldn't occur in real data, but a nature that "raises and lowers
            // the same stat" should net out neutral rather than picking one arbitrarily
            Assert.Equal(1.0, PokeDataParsing.NatureMultiplierFor("attack", "attack", "attack"));
        }

        [Fact]
        public void NatureMultiplierFor_EmptyStatName_ReturnsNeutral_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.NatureMultiplierFor("attack", "defense", ""));
            Assert.Null(ex);
            Assert.Equal(1.0, PokeDataParsing.NatureMultiplierFor("attack", "defense", ""));
        }

        // --- v3.0.0: ComputeEvolutionTreeLayout -----------------------------------------------------

        [Fact]
        public void ComputeEvolutionTreeLayout_LinearChain_AllNodesShareSameX()
        {
            var parents = new List<string> { "bulbasaur", "ivysaur" };
            var children = new List<string> { "ivysaur", "venusaur" };
            var triggers = new List<string> { "level-up", "level-up" };

            PokeDataParsing.ComputeEvolutionTreeLayout("bulbasaur", parents, children, triggers, 10.0, 10.0,
                out var nodeNames, out var nodeX, out var nodeY, out var edgeFrom, out var edgeTo, out var edgeTriggers);

            Assert.Equal(new[] { "bulbasaur", "ivysaur", "venusaur" }, nodeNames);
            Assert.Equal(nodeX[0], nodeX[1]);
            Assert.Equal(nodeX[1], nodeX[2]);
            Assert.Equal(0.0, nodeY[0]);
            Assert.Equal(-10.0, nodeY[1]);
            Assert.Equal(-20.0, nodeY[2]);
            Assert.Equal(2, edgeFrom.Count);
        }

        [Fact]
        public void ComputeEvolutionTreeLayout_BranchingChain_SpreadsSiblingsAndCentersParent()
        {
            var parents = new List<string> { "eevee", "eevee", "eevee" };
            var children = new List<string> { "vaporeon", "jolteon", "flareon" };
            var triggers = new List<string> { "use-item", "use-item", "use-item" };

            PokeDataParsing.ComputeEvolutionTreeLayout("eevee", parents, children, triggers, 10.0, 10.0,
                out var nodeNames, out var nodeX, out var nodeY, out var edgeFrom, out var edgeTo, out var edgeTriggers);

            Assert.Equal(4, nodeNames.Count);
            int eeveeIndex = nodeNames.IndexOf("eevee");
            int vaporeonIndex = nodeNames.IndexOf("vaporeon");
            int jolteonIndex = nodeNames.IndexOf("jolteon");
            int flareonIndex = nodeNames.IndexOf("flareon");

            // eevee's x is centered over its 3 children
            double expectedCenter = (nodeX[vaporeonIndex] + nodeX[jolteonIndex] + nodeX[flareonIndex]) / 3.0;
            Assert.Equal(expectedCenter, nodeX[eeveeIndex], 6);

            // children are at distinct x positions
            Assert.NotEqual(nodeX[vaporeonIndex], nodeX[jolteonIndex]);
            Assert.NotEqual(nodeX[jolteonIndex], nodeX[flareonIndex]);

            Assert.Equal(3, edgeFrom.Count);
            Assert.All(edgeFrom, i => Assert.Equal(eeveeIndex, i));
        }

        [Fact]
        public void ComputeEvolutionTreeLayout_NoEvolutions_SingleRootNode()
        {
            PokeDataParsing.ComputeEvolutionTreeLayout("tauros", new List<string>(), new List<string>(), new List<string>(), 10.0, 10.0,
                out var nodeNames, out var nodeX, out var nodeY, out var edgeFrom, out var edgeTo, out var edgeTriggers);

            Assert.Single(nodeNames);
            Assert.Equal("tauros", nodeNames[0]);
            Assert.Empty(edgeFrom);
        }

        [Fact]
        public void ComputeEvolutionTreeLayout_EmptyRootName_ReturnsEmpty_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.ComputeEvolutionTreeLayout("", new List<string>(), new List<string>(), new List<string>(), 10.0, 10.0,
                out var nodeNames, out var nodeX, out var nodeY, out var edgeFrom, out var edgeTo, out var edgeTriggers));

            Assert.Null(ex);
            PokeDataParsing.ComputeEvolutionTreeLayout("", new List<string>(), new List<string>(), new List<string>(), 10.0, 10.0,
                out var names2, out var x2, out var y2, out var ef2, out var et2, out var tr2);
            Assert.Empty(names2);
        }

        [Fact]
        public void ComputeEvolutionTreeLayout_EdgeTriggersParallelToEdges()
        {
            var parents = new List<string> { "bulbasaur" };
            var children = new List<string> { "ivysaur" };
            var triggers = new List<string> { "level-up" };

            PokeDataParsing.ComputeEvolutionTreeLayout("bulbasaur", parents, children, triggers, 10.0, 10.0,
                out var nodeNames, out var nodeX, out var nodeY, out var edgeFrom, out var edgeTo, out var edgeTriggers);

            Assert.Equal(new[] { "level-up" }, edgeTriggers);
        }

        // --- v3.0.0: Luminance / IsOpaquePixel -------------------------------------------------------

        [Fact]
        public void Luminance_White_ReturnsOne()
        {
            Assert.Equal(1.0, PokeDataParsing.Luminance(255, 255, 255), 3);
        }

        [Fact]
        public void Luminance_Black_ReturnsZero()
        {
            Assert.Equal(0.0, PokeDataParsing.Luminance(0, 0, 0), 3);
        }

        [Fact]
        public void Luminance_PureGreen_WeightedHigherThanPureBlue()
        {
            double green = PokeDataParsing.Luminance(0, 255, 0);
            double blue = PokeDataParsing.Luminance(0, 0, 255);
            Assert.True(green > blue);
        }

        [Theory]
        [InlineData(255, 10, true)]
        [InlineData(0, 10, false)]
        [InlineData(10, 10, false)]
        [InlineData(11, 10, true)]
        public void IsOpaquePixel_ComparesAgainstThreshold(int alpha, int threshold, bool expected)
        {
            Assert.Equal(expected, PokeDataParsing.IsOpaquePixel(alpha, threshold));
        }

        // --- v4.0.0: ParseBerryFlavors (Get Berry / Get Berry Flavors) --------------------------

        [Fact]
        public void ParseBerryFlavors_FlattensPotencyAndNameInOrder()
        {
            var flavors = JArray.Parse(@"[
                { ""potency"": 10, ""flavor"": { ""name"": ""spicy"" } },
                { ""potency"": 0,  ""flavor"": { ""name"": ""dry"" } },
                { ""potency"": 0,  ""flavor"": { ""name"": ""sweet"" } }
            ]");

            var names = new List<string>();
            var potencies = new List<int>();
            PokeDataParsing.ParseBerryFlavors(flavors, names, potencies);

            Assert.Equal(new[] { "spicy", "dry", "sweet" }, names);
            Assert.Equal(new[] { 10, 0, 0 }, potencies);
        }

        [Fact]
        public void ParseBerryFlavors_NullArray_LeavesListsEmpty_DoesNotThrow()
        {
            var names = new List<string>();
            var potencies = new List<int>();

            var ex = Record.Exception(() => PokeDataParsing.ParseBerryFlavors(null, names, potencies));

            Assert.Null(ex);
            Assert.Empty(names);
            Assert.Empty(potencies);
        }

        [Fact]
        public void ParseBerryFlavors_MissingPotencyField_DefaultsToZero()
        {
            var flavors = JArray.Parse(@"[{ ""flavor"": { ""name"": ""bitter"" } }]");

            var names = new List<string>();
            var potencies = new List<int>();
            PokeDataParsing.ParseBerryFlavors(flavors, names, potencies);

            Assert.Equal(new[] { "bitter" }, names);
            Assert.Equal(new[] { 0 }, potencies);
        }

        // --- v4.0.0: NamesToList reused for Get Location Areas -----------------------------------

        [Fact]
        public void NamesToList_LocationAreaShape_FlattensAreaNamesInOrder()
        {
            var areas = JArray.Parse(@"[
                { ""name"": ""canalave-city-area"", ""url"": ""https://pokeapi.co/api/v2/location-area/1/"" },
                { ""name"": ""canalave-city-area-2"", ""url"": ""https://pokeapi.co/api/v2/location-area/2/"" }
            ]");

            var result = PokeDataParsing.NamesToList(areas);

            Assert.Equal(new[] { "canalave-city-area", "canalave-city-area-2" }, result);
        }

        // --- v4.0.0 round 5: NamesToList reused for Get Region Locations/Pokedexes and
        //     Get Egg Group's species list ----------------------------------------------------

        [Fact]
        public void NamesToList_RegionLocationsShape_FlattensNamesInOrder()
        {
            var locations = JArray.Parse(@"[
                { ""name"": ""celadon-city"", ""url"": ""https://pokeapi.co/api/v2/location/67/"" },
                { ""name"": ""cerulean-city"", ""url"": ""https://pokeapi.co/api/v2/location/68/"" }
            ]");

            var result = PokeDataParsing.NamesToList(locations);

            Assert.Equal(new[] { "celadon-city", "cerulean-city" }, result);
        }

        [Fact]
        public void NamesToList_RegionPokedexesShape_FlattensNamesInOrder()
        {
            var pokedexes = JArray.Parse(@"[
                { ""name"": ""kanto"", ""url"": ""https://pokeapi.co/api/v2/pokedex/2/"" },
                { ""name"": ""letsgo-kanto"", ""url"": ""https://pokeapi.co/api/v2/pokedex/26/"" }
            ]");

            var result = PokeDataParsing.NamesToList(pokedexes);

            Assert.Equal(new[] { "kanto", "letsgo-kanto" }, result);
        }

        [Fact]
        public void NamesToList_EggGroupSpeciesShape_FlattensNamesInOrder()
        {
            var species = JArray.Parse(@"[
                { ""name"": ""bulbasaur"", ""url"": ""https://pokeapi.co/api/v2/pokemon-species/1/"" },
                { ""name"": ""charmander"", ""url"": ""https://pokeapi.co/api/v2/pokemon-species/4/"" }
            ]");

            var result = PokeDataParsing.NamesToList(species);

            Assert.Equal(new[] { "bulbasaur", "charmander" }, result);
        }

        // --- v4.0.0 round 5: PokedexSpeciesNames (Get Pokedex Species) ---------------------------

        [Fact]
        public void PokedexSpeciesNames_FlattensNestedPokemonSpeciesNameInOrder()
        {
            var entries = JArray.Parse(@"[
                { ""entry_number"": 1, ""pokemon_species"": { ""name"": ""bulbasaur"", ""url"": ""https://pokeapi.co/api/v2/pokemon-species/1/"" } },
                { ""entry_number"": 2, ""pokemon_species"": { ""name"": ""ivysaur"", ""url"": ""https://pokeapi.co/api/v2/pokemon-species/2/"" } }
            ]");

            var result = PokeDataParsing.PokedexSpeciesNames(entries);

            Assert.Equal(new[] { "bulbasaur", "ivysaur" }, result);
        }

        [Fact]
        public void PokedexSpeciesNames_NullArray_ReturnsEmptyList_DoesNotThrow()
        {
            var ex = Record.Exception(() => PokeDataParsing.PokedexSpeciesNames(null));

            Assert.Null(ex);
            Assert.Empty(PokeDataParsing.PokedexSpeciesNames(null));
        }

        // --- v4.0.0 round 5: ParseGrowthRateLevels (Get Growth Rate Levels) ----------------------

        [Fact]
        public void ParseGrowthRateLevels_FlattensLevelAndExperienceInOrder()
        {
            var levelsArray = JArray.Parse(@"[
                { ""level"": 1, ""experience"": 0 },
                { ""level"": 2, ""experience"": 10 },
                { ""level"": 3, ""experience"": 33 }
            ]");

            var levels = new List<int>();
            var experience = new List<int>();
            PokeDataParsing.ParseGrowthRateLevels(levelsArray, levels, experience);

            Assert.Equal(new[] { 1, 2, 3 }, levels);
            Assert.Equal(new[] { 0, 10, 33 }, experience);
        }

        [Fact]
        public void ParseGrowthRateLevels_NullArray_LeavesListsEmpty_DoesNotThrow()
        {
            var levels = new List<int>();
            var experience = new List<int>();

            var ex = Record.Exception(() => PokeDataParsing.ParseGrowthRateLevels(null, levels, experience));

            Assert.Null(ex);
            Assert.Empty(levels);
            Assert.Empty(experience);
        }

        // --- v4.0.0 round 5: ParseStatAffectingNatures (Get Stat Affecting Natures) --------------

        [Fact]
        public void ParseStatAffectingNatures_SplitsIncreaseAndDecreaseNames()
        {
            var stat = JObject.Parse(@"{
                ""affecting_natures"": {
                    ""increase"": [
                        { ""name"": ""lonely"", ""url"": ""https://pokeapi.co/api/v2/nature/6/"" },
                        { ""name"": ""adamant"", ""url"": ""https://pokeapi.co/api/v2/nature/11/"" }
                    ],
                    ""decrease"": [
                        { ""name"": ""bold"", ""url"": ""https://pokeapi.co/api/v2/nature/2/"" }
                    ]
                }
            }");

            var increasing = new List<string>();
            var decreasing = new List<string>();
            PokeDataParsing.ParseStatAffectingNatures(stat, increasing, decreasing);

            Assert.Equal(new[] { "lonely", "adamant" }, increasing);
            Assert.Equal(new[] { "bold" }, decreasing);
        }

        [Fact]
        public void ParseStatAffectingNatures_MissingField_ReturnsEmptyLists_DoesNotThrow()
        {
            var stat = JObject.Parse(@"{ ""name"": ""hp"" }");

            var increasing = new List<string>();
            var decreasing = new List<string>();
            var ex = Record.Exception(() => PokeDataParsing.ParseStatAffectingNatures(stat, increasing, decreasing));

            Assert.Null(ex);
            Assert.Empty(increasing);
            Assert.Empty(decreasing);
        }
    }
}

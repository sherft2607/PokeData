using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Pure parsing/flattening logic shared by every component that talks to PokeAPI.
    // No Grasshopper/RhinoCommon references, so this file can be compiled directly into the
    // plain net8.0 test project and exercised without a live API call or a GH runtime.
    // No nullable annotations here — this file also compiles under net48 (C# 7.3 default), which
    // does not support the `?` nullable-reference syntax; the test project suppresses the resulting
    // CS86xx warnings instead (see PokeData.Tests.csproj).
    public static class PokeDataParsing
    {
        #region Safe JSON navigation

        // A plain `token["key"]` indexer throws InvalidOperationException ("Cannot access child
        // value on Newtonsoft.Json.Linq.JValue") when `token` is a leaf value — which includes a
        // field whose JSON value is literally `null` (that deserializes to a JValue with
        // Type == Null, not a C# null reference, so `?.`/`?[]` alone does not guard it). Every
        // multi-hop lookup in this file and every component goes through these helpers instead of
        // a raw indexer chain.

        // item — descends one property, but only if token is actually a JObject; never throws
        public static JToken SafeChild(JToken token, string key)
        {
            var obj = token as JObject;
            return obj?[key];
        }

        // record — descends a chain of property names, stopping (returning null) at the first
        // hop that isn't a JObject (including a JSON-null-valued field)
        public static JToken SafePath(JToken token, params string[] path)
        {
            JToken current = token;
            foreach (var key in path)
                current = SafeChild(current, key);
            return current;
        }

        // item — string at the end of a safe path; JSON null or a missing/wrong-shape hop -> ""
        public static string SafeString(JToken token, params string[] path)
        {
            var result = SafePath(token, path);
            if (result == null || result.Type == JTokenType.Null) return "";
            return result.Type == JTokenType.String ? (string)result : result.ToString();
        }

        // item — array at the end of a safe path; null/wrong-shape -> null (callers already
        // treat a null JArray as "nothing to iterate")
        public static JArray SafeArray(JToken token, params string[] path)
        {
            return SafePath(token, path) as JArray;
        }

        #endregion

        // item
        public static HashSet<string> NamesOf(JArray array)
        {
            var set = new HashSet<string>();
            if (array == null) return set;
            foreach (var item in array)
                set.Add(SafeString(item, "name"));
            return set;
        }

        // item
        public static double MultiplierFor(HashSet<string> doubleTo, HashSet<string> halfTo, HashSet<string> noTo, string defendingType)
        {
            if (noTo.Contains(defendingType)) return 0.0;
            if (doubleTo.Contains(defendingType)) return 2.0;
            if (halfTo.Contains(defendingType)) return 0.5;
            return 1.0;
        }

        // record — flattens one type's already-parsed damage_relations against every type name given
        public static void FlattenTypeRow(
            string attackingTypeName,
            JObject parsedType,
            IReadOnlyList<string> allTypeNames,
            List<string> attacking,
            List<string> defending,
            List<double> multiplier)
        {
            var doubleTo = NamesOf(SafeArray(parsedType, "damage_relations", "double_damage_to"));
            var halfTo = NamesOf(SafeArray(parsedType, "damage_relations", "half_damage_to"));
            var noTo = NamesOf(SafeArray(parsedType, "damage_relations", "no_damage_to"));

            foreach (var defendingType in allTypeNames)
            {
                attacking.Add(attackingTypeName);
                defending.Add(defendingType);
                multiplier.Add(MultiplierFor(doubleTo, halfTo, noTo, defendingType));
            }
        }

        // item — flattens an array of {name} objects (e.g. egg_groups) into a plain list, in order
        public static List<string> NamesToList(JArray array)
        {
            var list = new List<string>();
            if (array == null) return list;
            foreach (var item in array)
                list.Add(SafeString(item, "name"));
            return list;
        }

        // item — a type's "pokemon" field nests one level deeper: [{ pokemon: { name, url } }, ...]
        public static List<string> AssociatedPokemonNames(JArray pokemonArray)
        {
            var list = new List<string>();
            if (pokemonArray == null) return list;
            foreach (var p in pokemonArray)
                list.Add(SafeString(p, "pokemon", "name"));
            return list;
        }

        // item — extract a pokemon response's two cry audio URLs; "cries" itself, or either
        // sub-field, may be JSON null on some entries (observed on live Gen 6+ lookups)
        public static void ParseCries(JObject parsedPokemon, out string legacyCryUrl, out string latestCryUrl)
        {
            legacyCryUrl = SafeString(parsedPokemon, "cries", "legacy");
            latestCryUrl = SafeString(parsedPokemon, "cries", "latest");
        }

        // record — pairs a pokemon's "stats" array into parallel name/value lists, in API order
        public static void ParseStats(JArray statsArray, List<string> names, List<int> values)
        {
            if (statsArray == null) return;
            foreach (var s in statsArray)
            {
                names.Add(SafeString(s, "stat", "name"));
                var baseStat = SafeChild(s, "base_stat");
                values.Add(baseStat != null && baseStat.Type != JTokenType.Null ? (int)baseStat : 0);
            }
        }

        // record — recursively walks an evolution-chain's "chain" node, emitting one edge per step
        public static void FlattenEvolutionChain(JObject node, List<string> parents, List<string> children, List<string> triggers)
        {
            if (node == null) return;

            string parentName = SafeString(node, "species", "name");
            var evolvesTo = SafeArray(node, "evolves_to");
            if (evolvesTo == null) return;

            foreach (var childNode in evolvesTo)
            {
                var childObj = childNode as JObject;
                if (childObj == null) continue;

                string childName = SafeString(childObj, "species", "name");
                string trigger = "";
                var details = SafeArray(childObj, "evolution_details");
                if (details != null && details.Count > 0)
                    trigger = SafeString(details[0], "trigger", "name");

                parents.Add(parentName);
                children.Add(childName);
                triggers.Add(trigger);

                FlattenEvolutionChain(childObj, parents, children, triggers);
            }
        }
    }
}

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Pure parsing/flattening logic shared by GetTypeMatrixComponent and GetEvolutionChainComponent.
    // No Grasshopper/RhinoCommon references, so this file can be compiled directly into the
    // plain net8.0 test project and exercised without a live API call or a GH runtime.
    // No nullable annotations here — this file also compiles under net48 (C# 7.3 default), which
    // does not support the `?` nullable-reference syntax; the test project suppresses the resulting
    // CS86xx warnings instead (see PokeData.Tests.csproj).
    public static class PokeDataParsing
    {
        // item
        public static HashSet<string> NamesOf(JArray array)
        {
            var set = new HashSet<string>();
            if (array == null) return set;
            foreach (var item in array)
                set.Add((string)item["name"] ?? "");
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
            var relations = parsedType?["damage_relations"];
            var doubleTo = NamesOf(relations?["double_damage_to"] as JArray);
            var halfTo = NamesOf(relations?["half_damage_to"] as JArray);
            var noTo = NamesOf(relations?["no_damage_to"] as JArray);

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
                list.Add((string)item["name"] ?? "");
            return list;
        }

        // item — a type's "pokemon" field nests one level deeper: [{ pokemon: { name, url } }, ...]
        public static List<string> AssociatedPokemonNames(JArray pokemonArray)
        {
            var list = new List<string>();
            if (pokemonArray == null) return list;
            foreach (var p in pokemonArray)
                list.Add((string)p["pokemon"]?["name"] ?? "");
            return list;
        }

        // item — extract a pokemon response's two cry audio URLs
        public static void ParseCries(JObject parsedPokemon, out string legacyCryUrl, out string latestCryUrl)
        {
            legacyCryUrl = "";
            latestCryUrl = "";
            if (parsedPokemon == null) return;
            legacyCryUrl = (string)parsedPokemon["cries"]?["legacy"] ?? "";
            latestCryUrl = (string)parsedPokemon["cries"]?["latest"] ?? "";
        }

        // record — pairs a pokemon's "stats" array into parallel name/value lists, in API order
        public static void ParseStats(JArray statsArray, List<string> names, List<int> values)
        {
            if (statsArray == null) return;
            foreach (var s in statsArray)
            {
                names.Add((string)s["stat"]?["name"] ?? "");
                values.Add((int?)s["base_stat"] ?? 0);
            }
        }

        // record — recursively walks an evolution-chain's "chain" node, emitting one edge per step
        public static void FlattenEvolutionChain(JObject node, List<string> parents, List<string> children, List<string> triggers)
        {
            if (node == null) return;

            string parentName = (string)node["species"]?["name"] ?? "";
            var evolvesTo = node["evolves_to"] as JArray;
            if (evolvesTo == null) return;

            foreach (var childNode in evolvesTo)
            {
                var childObj = childNode as JObject;
                if (childObj == null) continue;

                string childName = (string)childObj["species"]?["name"] ?? "";
                string trigger = "";
                var details = childObj["evolution_details"] as JArray;
                if (details != null && details.Count > 0)
                    trigger = (string)details[0]["trigger"]?["name"] ?? "";

                parents.Add(parentName);
                children.Add(childName);
                triggers.Add(trigger);

                FlattenEvolutionChain(childObj, parents, children, triggers);
            }
        }
    }
}

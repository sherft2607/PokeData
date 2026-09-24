using System;
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

        // record — v2.0.0: collects the first localized text entry per distinct language from an
        // entries array shaped [{ <textKey>: "...", language: { name: "en" } }, ...]
        // (move "flavor_text_entries"/"flavor_text", ability "effect_entries"/"short_effect") —
        // first entry per language wins, so a per-version-group array like flavor_text_entries
        // still yields one row per language rather than one per version
        public static void ParseLocalizedEntries(JArray entries, string textKey, out List<string> languages, out List<string> texts)
        {
            languages = new List<string>();
            texts = new List<string>();
            if (entries == null) return;

            var seen = new HashSet<string>();
            foreach (var entry in entries)
            {
                string lang = SafeString(entry, "language", "name");
                if (string.IsNullOrEmpty(lang) || !seen.Add(lang)) continue;
                languages.Add(lang);
                texts.Add(SafeString(entry, textKey));
            }
        }

        // record — v2.0.0: normalizes stat values onto a 2D radar/spider polygon, one vertex per
        // stat, evenly spaced starting at 12 o'clock and going clockwise. Pure math (no
        // Rhino.Geometry reference) so this stays testable in the plain net8.0 test project;
        // the component converts (x, y) pairs to Point3d.
        public static void ComputeStatRadarPoints(IReadOnlyList<int> statValues, double radius, double maxStat, out List<double> x, out List<double> y)
        {
            x = new List<double>();
            y = new List<double>();
            if (statValues == null || statValues.Count == 0 || maxStat <= 0) return;

            int n = statValues.Count;
            for (int i = 0; i < n; i++)
            {
                double normalized = Math.Max(0.0, Math.Min(1.0, statValues[i] / maxStat));
                double r = normalized * radius;
                double angle = -Math.PI / 2 + (2 * Math.PI * i / n);
                x.Add(r * Math.Cos(angle));
                y.Add(r * Math.Sin(angle));
            }
        }

        // item — v2.1.0: extracts the trailing numeric ID from a PokeAPI resource URL,
        // e.g. "https://pokeapi.co/api/v2/pokemon-species/1/" -> 1. Malformed/non-numeric -> 0.
        public static int ExtractIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return 0;
            var trimmed = url.TrimEnd('/');
            var idx = trimmed.LastIndexOf('/');
            if (idx < 0 || idx == trimmed.Length - 1) return 0;
            var idPart = trimmed.Substring(idx + 1);
            return int.TryParse(idPart, out var id) ? id : 0;
        }

        // record — v2.1.0: extracts the trailing numeric ID from every {..., url} entry in an
        // array (e.g. generation's "pokemon_species"), in order
        public static List<int> ExtractIdsFromUrls(JArray array, string urlKey = "url")
        {
            var list = new List<int>();
            if (array == null) return list;
            foreach (var item in array)
                list.Add(ExtractIdFromUrl(SafeString(item, urlKey)));
            return list;
        }

        // item — v3.0.0: combines up to two independent type defenses into one defensive
        // multiplier per attacking type (in allTypeNames order). Passing null for the second
        // type's sets treats it as a single-type Pokemon (multiplier 1.0 contributed). Shared by
        // ComputeDualTypeDefense's bucketing and Team Synergy's per-member heatmap row.
        public static double[] ComputeDefenseMultipliers(
            HashSet<string> doubleFrom1, HashSet<string> halfFrom1, HashSet<string> noFrom1,
            HashSet<string> doubleFrom2, HashSet<string> halfFrom2, HashSet<string> noFrom2,
            IReadOnlyList<string> allTypeNames)
        {
            var result = new double[allTypeNames?.Count ?? 0];
            if (allTypeNames == null) return result;

            var emptySet = new HashSet<string>();
            for (int i = 0; i < allTypeNames.Count; i++)
            {
                var attackingType = allTypeNames[i];
                double m1 = MultiplierFor(doubleFrom1 ?? emptySet, halfFrom1 ?? emptySet, noFrom1 ?? emptySet, attackingType);
                double m2 = doubleFrom2 == null ? 1.0 : MultiplierFor(doubleFrom2, halfFrom2 ?? emptySet, noFrom2 ?? emptySet, attackingType);
                result[i] = m1 * m2;
            }
            return result;
        }

        // record — v2.1.0: combines two independent type defenses (attacker deals X to this type)
        // into a dual-type defensive multiplier per attacking type, then buckets attacking types
        // by the resulting multiplier (4x/2x/1x/0.5x/0.25x/0x). Passing null for the second type's
        // sets treats it as a single-type Pokemon (multiplier 1.0 contributed).
        public static void ComputeDualTypeDefense(
            HashSet<string> doubleFrom1, HashSet<string> halfFrom1, HashSet<string> noFrom1,
            HashSet<string> doubleFrom2, HashSet<string> halfFrom2, HashSet<string> noFrom2,
            IReadOnlyList<string> allTypeNames,
            List<string> x4, List<string> x2, List<string> x1, List<string> h2, List<string> h4, List<string> x0)
        {
            if (allTypeNames == null) return;

            var multipliers = ComputeDefenseMultipliers(doubleFrom1, halfFrom1, noFrom1, doubleFrom2, halfFrom2, noFrom2, allTypeNames);

            for (int i = 0; i < allTypeNames.Count; i++)
            {
                var attackingType = allTypeNames[i];
                double total = multipliers[i];

                if (total == 0.0) x0.Add(attackingType);
                else if (total == 0.25) h4.Add(attackingType);
                else if (total == 0.5) h2.Add(attackingType);
                else if (total == 1.0) x1.Add(attackingType);
                else if (total == 2.0) x2.Add(attackingType);
                else if (total == 4.0) x4.Add(attackingType);
            }
        }

        // record — v3.0.0: given each team member's per-attacking-type defense multiplier array
        // (aligned to allTypeNames, from ComputeDefenseMultipliers), computes team-wide coverage
        // gaps (an attacking type every member is weak, 2x+, to) and a composite vulnerability
        // score per attacking type (mean multiplier across the team).
        public static void ComputeTeamSynergy(
            IReadOnlyList<double[]> perMemberMultipliers,
            IReadOnlyList<string> allTypeNames,
            out List<string> coverageGaps,
            out List<double> compositeVulnerability)
        {
            coverageGaps = new List<string>();
            compositeVulnerability = new List<double>();
            if (allTypeNames == null) return;

            for (int t = 0; t < allTypeNames.Count; t++)
            {
                double sum = 0.0;
                bool allWeak = perMemberMultipliers != null && perMemberMultipliers.Count > 0;

                if (perMemberMultipliers != null)
                {
                    foreach (var memberMultipliers in perMemberMultipliers)
                    {
                        double m = (memberMultipliers != null && t < memberMultipliers.Length) ? memberMultipliers[t] : 1.0;
                        sum += m;
                        if (m < 2.0) allWeak = false;
                    }
                }

                compositeVulnerability.Add(perMemberMultipliers != null && perMemberMultipliers.Count > 0 ? sum / perMemberMultipliers.Count : 0.0);
                if (allWeak) coverageGaps.Add(allTypeNames[t]);
            }
        }

        // item — v3.0.0: exact HP formula (Generation III+): floor(((2*Base + IV + floor(EV/4)) *
        // Level) / 100) + Level + 10. Integer division throughout matches in-game truncation.
        public static int ComputeHpStat(int baseStat, int iv, int ev, int level)
        {
            return ((2 * baseStat + iv + ev / 4) * level) / 100 + level + 10;
        }

        // item — v3.0.0: exact non-HP stat formula (Generation III+): floor((floor(((2*Base + IV +
        // floor(EV/4)) * Level) / 100) + 5) * NatureMultiplier)
        public static int ComputeBattleStat(int baseStat, int iv, int ev, int level, double natureMultiplier)
        {
            int raw = ((2 * baseStat + iv + ev / 4) * level) / 100 + 5;
            return (int)(raw * natureMultiplier);
        }

        // item — v3.0.0: 1.1 if this nature raises statName, 0.9 if it lowers statName, else 1.0.
        // A nature whose increased/decreased stat are the same (shouldn't happen in real data, but
        // defensively handled) is treated as neutral, matching in-game behavior for "neutral" natures.
        public static double NatureMultiplierFor(string increasedStat, string decreasedStat, string statName)
        {
            if (string.IsNullOrEmpty(statName)) return 1.0;
            if (!string.IsNullOrEmpty(increasedStat) && !string.IsNullOrEmpty(decreasedStat) && increasedStat == decreasedStat) return 1.0;
            if (string.Equals(increasedStat, statName, StringComparison.OrdinalIgnoreCase)) return 1.1;
            if (string.Equals(decreasedStat, statName, StringComparison.OrdinalIgnoreCase)) return 0.9;
            return 1.0;
        }

        // record — v3.0.0: lays out an evolution chain's parent/child/trigger edges (as produced
        // by FlattenEvolutionChain) as a simple tree: depth by BFS distance from the root, x by
        // post-order layout (leaves placed left-to-right, each internal node centered over its
        // children) — pure (x, y) pairs, no Rhino.Geometry reference, so this stays testable.
        // Node names must be unique within one chain (true for real evolution-chain data).
        public static void ComputeEvolutionTreeLayout(
            string rootName,
            IReadOnlyList<string> parents,
            IReadOnlyList<string> children,
            IReadOnlyList<string> triggers,
            double horizontalSpacing,
            double verticalSpacing,
            out List<string> nodeNames,
            out List<double> nodeX,
            out List<double> nodeY,
            out List<int> edgeFromIndex,
            out List<int> edgeToIndex,
            out List<string> edgeTriggers)
        {
            nodeNames = new List<string>();
            nodeX = new List<double>();
            nodeY = new List<double>();
            edgeFromIndex = new List<int>();
            edgeToIndex = new List<int>();
            edgeTriggers = new List<string>();

            if (string.IsNullOrEmpty(rootName)) return;

            var childrenOf = new Dictionary<string, List<int>>();
            int edgeCount = (parents != null) ? parents.Count : 0;
            for (int i = 0; i < edgeCount; i++)
            {
                var p = parents[i];
                if (!childrenOf.TryGetValue(p, out var list))
                {
                    list = new List<int>();
                    childrenOf[p] = list;
                }
                list.Add(i);
            }

            nodeNames.Add(rootName);
            var indexOf = new Dictionary<string, int> { { rootName, 0 } };
            var depthOf = new Dictionary<string, int> { { rootName, 0 } };

            // BFS to register every node once, assign depth, and record edges by node index
            var queue = new Queue<string>();
            queue.Enqueue(rootName);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!childrenOf.TryGetValue(current, out var edgeIndices)) continue;

                foreach (var edgeIndex in edgeIndices)
                {
                    var childName = children[edgeIndex];
                    if (!indexOf.ContainsKey(childName))
                    {
                        indexOf[childName] = nodeNames.Count;
                        depthOf[childName] = depthOf[current] + 1;
                        nodeNames.Add(childName);
                        queue.Enqueue(childName);
                    }

                    edgeFromIndex.Add(indexOf[current]);
                    edgeToIndex.Add(indexOf[childName]);
                    edgeTriggers.Add(triggers != null && edgeIndex < triggers.Count ? triggers[edgeIndex] : "");
                }
            }

            // Post-order x layout: leaves get sequential slots, internal nodes center over children
            var names = nodeNames; // local copy — an `out` parameter can't be captured by the local function below
            var x = new double[names.Count];
            var visited = new bool[names.Count];
            double nextLeafSlot = 0.0;

            void AssignX(int nodeIndex)
            {
                if (visited[nodeIndex]) return;
                visited[nodeIndex] = true;

                var nodeName = names[nodeIndex];
                if (!childrenOf.TryGetValue(nodeName, out var edgeIndices) || edgeIndices.Count == 0)
                {
                    x[nodeIndex] = nextLeafSlot;
                    nextLeafSlot += horizontalSpacing;
                    return;
                }

                double sum = 0.0;
                int count = 0;
                foreach (var edgeIndex in edgeIndices)
                {
                    var childIndex = indexOf[children[edgeIndex]];
                    AssignX(childIndex);
                    sum += x[childIndex];
                    count++;
                }
                x[nodeIndex] = count > 0 ? sum / count : nextLeafSlot;
            }

            AssignX(0);

            for (int i = 0; i < names.Count; i++)
            {
                nodeX.Add(x[i]);
                nodeY.Add(-depthOf[names[i]] * verticalSpacing);
            }
        }

        // item — v2.1.0: PokeAPI height (decimetres) / weight (hectograms) -> metric + imperial
        public static void ConvertDimensions(double heightDecimetres, double weightHectograms,
            out double heightMeters, out double heightFeet, out double weightKg, out double weightLb)
        {
            heightMeters = heightDecimetres * 0.1;
            heightFeet = heightMeters * 3.28084;
            weightKg = weightHectograms * 0.1;
            weightLb = weightKg * 2.20462;
        }

        // record — v2.1.0: evenly-spaced points on a fixed-radius regular polygon (unlike
        // ComputeStatRadarPoints, radius does not vary per vertex) — the base/top ring geometry
        // for Stat Mesh 3D's extruded prism
        public static void ComputeRegularPolygonPoints(int sides, double radius, out List<double> x, out List<double> y)
        {
            x = new List<double>();
            y = new List<double>();
            if (sides < 3 || radius <= 0) return;

            for (int i = 0; i < sides; i++)
            {
                double angle = -Math.PI / 2 + (2 * Math.PI * i / sides);
                x.Add(radius * Math.Cos(angle));
                y.Add(radius * Math.Sin(angle));
            }
        }

        // item — v2.1.0: normalizes stat values to [0,1] then scales by heightFactor, for Stat
        // Mesh 3D's per-vertex extrusion height
        public static List<double> ComputeStatMeshHeights(IReadOnlyList<int> statValues, double maxStat, double heightFactor)
        {
            var heights = new List<double>();
            if (statValues == null || maxStat <= 0) return heights;

            foreach (var v in statValues)
            {
                double normalized = Math.Max(0.0, Math.Min(1.0, v / maxStat));
                heights.Add(normalized * heightFactor);
            }
            return heights;
        }

        // record — v2.1.0: filters two parallel lists (e.g. names + stat totals) by a named rule
        // against [min, max]; unknown rule names fall back to "between". Extra items in the
        // longer list are ignored rather than throwing.
        public static void FilterByRule(IReadOnlyList<string> names, IReadOnlyList<double> values, string rule, double min, double max,
            out List<string> filteredNames, out List<double> filteredValues, out List<int> matchIndices)
        {
            filteredNames = new List<string>();
            filteredValues = new List<double>();
            matchIndices = new List<int>();
            if (names == null || values == null) return;

            int n = Math.Min(names.Count, values.Count);
            string normalizedRule = (rule ?? "").Trim().ToLowerInvariant();

            for (int i = 0; i < n; i++)
            {
                double v = values[i];
                bool match;
                switch (normalizedRule)
                {
                    case "greaterthan": match = v > min; break;
                    case "lessthan": match = v < max; break;
                    case "equals": match = Math.Abs(v - min) < 1e-9; break;
                    case "between":
                    default: match = v >= min && v <= max; break;
                }

                if (match)
                {
                    filteredNames.Add(names[i]);
                    filteredValues.Add(v);
                    matchIndices.Add(i);
                }
            }
        }

        // item — v2.1.0: joins a name + stat display lines into one label string for
        // Canvas Sprite Card, trimming trailing blank lines. Empty inputs -> "".
        public static string BuildCardLabel(string name, IReadOnlyList<string> statLines)
        {
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrWhiteSpace(name)) sb.AppendLine(name);
            if (statLines != null)
                foreach (var line in statLines)
                    sb.AppendLine(line);
            return sb.ToString().TrimEnd('\r', '\n');
        }

        // item — v2.1.0: one-line summary of a batch download's outcome
        public static string SummarizeBatchStatus(int successCount, int failureCount)
        {
            if (failureCount == 0) return "OK (" + successCount + "/" + successCount + ")";
            return "Partial: " + successCount + " succeeded, " + failureCount + " failed";
        }

        // item — v3.0.0: perceptual luminance (Rec. 709 coefficients) of an 8-bit RGB triple,
        // normalized to [0,1] — drives Sprite To Voxel's heightfield relief.
        public static double Luminance(int r, int g, int b)
        {
            return (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255.0;
        }

        // item — v3.0.0: whether a pixel counts as "solid" for voxelization, given its alpha and
        // a threshold (0-255) below which a pixel is treated as background/transparent
        public static bool IsOpaquePixel(int alpha, int threshold)
        {
            return alpha > threshold;
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

        // Flattens a berry's flavors[] array ({potency, flavor:{name}} pairs) into two
        // positionally-parallel lists. Berries always carry all 5 flavors (spicy/dry/sweet/
        // bitter/sour), potency 0 meaning "not present" rather than the entry being absent.
        public static void ParseBerryFlavors(JArray flavorsArray, List<string> names, List<int> potencies)
        {
            if (flavorsArray == null) return;
            foreach (var entry in flavorsArray)
            {
                names.Add(SafeString(entry, "flavor", "name"));
                potencies.Add((int?)SafeChild(entry, "potency") ?? 0);
            }
        }

        // item — a pokedex's pokemon_entries[] nests one level deeper than NamesToList expects:
        // [{ entry_number, pokemon_species: { name, url } }, ...]. Returned in the array's own
        // order (PokeAPI already orders these by entry_number).
        public static List<string> PokedexSpeciesNames(JArray pokemonEntries)
        {
            var list = new List<string>();
            if (pokemonEntries == null) return list;
            foreach (var entry in pokemonEntries)
                list.Add(SafeString(entry, "pokemon_species", "name"));
            return list;
        }

        // record — a growth-rate's levels[] array ({level, experience} pairs) into two
        // positionally-parallel lists, in the array's own order (PokeAPI already orders by level).
        public static void ParseGrowthRateLevels(JArray levelsArray, List<int> levels, List<int> experience)
        {
            if (levelsArray == null) return;
            foreach (var entry in levelsArray)
            {
                levels.Add((int?)SafeChild(entry, "level") ?? 0);
                experience.Add((int?)SafeChild(entry, "experience") ?? 0);
            }
        }

        // record — a stat's affecting_natures.{increase,decrease}[] ({name, url} pairs, same
        // shape NamesToList already handles) into two separate name lists.
        public static void ParseStatAffectingNatures(JObject parsedStat, List<string> increasing, List<string> decreasing)
        {
            increasing.AddRange(NamesToList(SafeArray(parsedStat, "affecting_natures", "increase")));
            decreasing.AddRange(NamesToList(SafeArray(parsedStat, "affecting_natures", "decrease")));
        }
    }
}

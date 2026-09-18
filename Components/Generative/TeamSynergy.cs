using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Takes up to 6 team members as parallel Names/Type 1s/Type 2s lists and computes a full
    // team-by-attacking-type defensive heatmap (flattened to 3 parallel lists, same convention as
    // Get Type Matrix), team coverage gaps (an attacking type every member is weak to), and a
    // composite vulnerability score per attacking type.
    public class TeamSynergyComponent : GH_Component
    {
        public TeamSynergyComponent()
          : base("Team Synergy", "Synergy",
              "Computes a team's defensive heatmap, coverage gaps, and composite vulnerability across all 18 types.",
              PluginUtilities.TabName, PluginUtilities.CategoryAKGenerative)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "N", "Team member names (1-6), e.g. Get Pokemon Batch's input list.", GH_ParamAccess.list);
            pManager.AddTextParameter("Type 1s", "T1", "First type per member, parallel to Names.", GH_ParamAccess.list);
            pManager.AddTextParameter("Type 2s", "T2", "Second type per member, parallel to Names — empty string for a single-type member.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Member", "M", "Member name per heatmap row (Names.Count x 18 rows).", GH_ParamAccess.list);
            pManager.AddTextParameter("Attacking Type", "AT", "Attacking type per heatmap row, parallel to Member.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Multiplier", "MU", "That member's defensive multiplier against that attacking type, parallel to Member.", GH_ParamAccess.list);
            pManager.AddTextParameter("Coverage Gaps", "CG", "Attacking types every team member is weak (2x+) to.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Composite Vulnerability", "CV", "Mean defensive multiplier across the team, one per attacking type (18 items, same order as Get Type Matrix).", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            var names = new List<string>();
            var type1s = new List<string>();
            var type2s = new List<string>();

            // 2. DA.GetData calls
            DA.GetDataList(0, names);
            DA.GetDataList(1, type1s);
            DA.GetDataList(2, type2s);

            // 3. Guard clauses / clamping
            if (names.Count == 0 || names.Count > 6)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Names must have between 1 and 6 entries.");
                DA.SetData(5, "Error: Names must have between 1 and 6 entries.");
                return;
            }

            if (type1s.Count != names.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Type 1s must be parallel to Names.");
                DA.SetData(5, "Error: Type 1s must be parallel to Names.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var typeCache = new Dictionary<string, JObject>();

            JObject FetchType(string typeName)
            {
                if (string.IsNullOrWhiteSpace(typeName)) return null;
                var key = typeName.Trim().ToLowerInvariant();
                if (typeCache.TryGetValue(key, out var cached)) return cached;

                var (ok, json, err) = Task.Run(() => client.GetTypeAsync(key)).GetAwaiter().GetResult();
                if (!ok)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not fetch type \"" + key + "\": " + err);
                    typeCache[key] = null;
                    return null;
                }

                JObject parsed = null;
                try { parsed = JObject.Parse(json); }
                catch (JsonException) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Type \"" + key + "\" response was not JSON."); }

                typeCache[key] = parsed;
                return parsed;
            }

            var perMemberMultipliers = new List<double[]>();
            var memberRow = new List<string>();
            var attackingTypeRow = new List<string>();
            var multiplierRow = new List<double>();

            for (int m = 0; m < names.Count; m++)
            {
                string type1 = type1s[m];
                string type2 = m < type2s.Count ? type2s[m] : "";

                var parsed1 = FetchType(type1);
                var parsed2 = string.IsNullOrWhiteSpace(type2) ? null : FetchType(type2);

                HashSet<string> doubleFrom1 = null, halfFrom1 = null, noFrom1 = null;
                if (parsed1 != null)
                {
                    doubleFrom1 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed1, "damage_relations", "double_damage_from"));
                    halfFrom1 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed1, "damage_relations", "half_damage_from"));
                    noFrom1 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed1, "damage_relations", "no_damage_from"));
                }

                HashSet<string> doubleFrom2 = null, halfFrom2 = null, noFrom2 = null;
                if (parsed2 != null)
                {
                    doubleFrom2 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed2, "damage_relations", "double_damage_from"));
                    halfFrom2 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed2, "damage_relations", "half_damage_from"));
                    noFrom2 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed2, "damage_relations", "no_damage_from"));
                }

                var multipliers = PokeDataParsing.ComputeDefenseMultipliers(
                    doubleFrom1, halfFrom1, noFrom1, doubleFrom2, halfFrom2, noFrom2, PluginUtilities.AllTypeNames);
                perMemberMultipliers.Add(multipliers);

                for (int t = 0; t < PluginUtilities.AllTypeNames.Length; t++)
                {
                    memberRow.Add(names[m]);
                    attackingTypeRow.Add(PluginUtilities.AllTypeNames[t]);
                    multiplierRow.Add(multipliers[t]);
                }
            }

            PokeDataParsing.ComputeTeamSynergy(perMemberMultipliers, PluginUtilities.AllTypeNames,
                out var coverageGaps, out var compositeVulnerability);

            // 5. DA.SetData calls
            DA.SetDataList(0, memberRow);
            DA.SetDataList(1, attackingTypeRow);
            DA.SetDataList(2, multiplierRow);
            DA.SetDataList(3, coverageGaps);
            DA.SetDataList(4, compositeVulnerability);
            DA.SetData(5, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_TeamSynergy;
        public override Guid ComponentGuid => new Guid("8ca22794-70fe-4db3-82af-57b0af51d121");
    }
}

using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Combines two independent types' defensive relations into a dual-type Pokemon's true
    // defensive multipliers (4x/2x/1x/0.5x/0.25x/0x) against every attacking type. Type 2 is
    // optional — leave it empty for a single-type Pokemon.
    public class DualTypeMatchupComponent : GH_Component
    {
        public DualTypeMatchupComponent()
          : base("Dual Type Matchup", "DualType",
              "Combines two types' defenses into a dual-type Pokemon's true damage multipliers against every attacking type.",
              PluginUtilities.TabName, PluginUtilities.CategoryABTypes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Type 1", "T1", "First (or only) elemental type name.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Type 2", "T2", "Second elemental type name — leave empty for a single-type Pokemon.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("4x Damage From", "X4", "Attacking types that deal 4x damage to this type combination.", GH_ParamAccess.list);
            pManager.AddTextParameter("2x Damage From", "X2", "Attacking types that deal 2x damage.", GH_ParamAccess.list);
            pManager.AddTextParameter("1x Damage From", "X1", "Attacking types that deal neutral (1x) damage.", GH_ParamAccess.list);
            pManager.AddTextParameter("0.5x Damage From", "H2", "Attacking types that deal 0.5x damage.", GH_ParamAccess.list);
            pManager.AddTextParameter("0.25x Damage From", "H4", "Attacking types that deal 0.25x damage.", GH_ParamAccess.list);
            pManager.AddTextParameter("0x Damage From", "X0", "Attacking types this type combination is immune to.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON responses (both types, as a JSON array).", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string type1 = "", type2 = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref type1);
            DA.GetData(1, ref type2);

            // 3. Guard clauses / clamping
            if (string.IsNullOrWhiteSpace(type1))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Type 1 is empty.");
                DA.SetData(6, "Error: Type 1 is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok1, json1, err1) = Task.Run(() => client.GetTypeAsync(type1)).GetAwaiter().GetResult();

            if (!ok1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err1);
                DA.SetData(6, "Error: " + err1);
                return;
            }

            string json2 = "";
            bool hasType2 = !string.IsNullOrWhiteSpace(type2);
            if (hasType2)
            {
                var (ok2, j2, err2) = Task.Run(() => client.GetTypeAsync(type2)).GetAwaiter().GetResult();
                if (!ok2)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err2);
                    DA.SetData(6, "Error: " + err2);
                    return;
                }
                json2 = j2;
            }

            JObject parsed1 = null, parsed2 = null;
            try { parsed1 = JObject.Parse(json1); } catch (JsonException) { }
            if (hasType2) { try { parsed2 = JObject.Parse(json2); } catch (JsonException) { } }

            var x4 = new List<string>(); var x2 = new List<string>(); var x1 = new List<string>();
            var h2 = new List<string>(); var h4 = new List<string>(); var x0 = new List<string>();

            if (parsed1 != null)
            {
                var doubleFrom1 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed1, "damage_relations", "double_damage_from"));
                var halfFrom1 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed1, "damage_relations", "half_damage_from"));
                var noFrom1 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed1, "damage_relations", "no_damage_from"));

                HashSet<string> doubleFrom2 = null, halfFrom2 = null, noFrom2 = null;
                if (parsed2 != null)
                {
                    doubleFrom2 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed2, "damage_relations", "double_damage_from"));
                    halfFrom2 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed2, "damage_relations", "half_damage_from"));
                    noFrom2 = PokeDataParsing.NamesOf(PokeDataParsing.SafeArray(parsed2, "damage_relations", "no_damage_from"));
                }

                PokeDataParsing.ComputeDualTypeDefense(
                    doubleFrom1, halfFrom1, noFrom1,
                    doubleFrom2, halfFrom2, noFrom2,
                    PluginUtilities.AllTypeNames,
                    x4, x2, x1, h2, h4, x0);
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, x4);
            DA.SetDataList(1, x2);
            DA.SetDataList(2, x1);
            DA.SetDataList(3, h2);
            DA.SetDataList(4, h4);
            DA.SetDataList(5, x0);
            DA.SetData(6, "OK");
            DA.SetData(7, new JArray(json1, json2).ToString());
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_DualTypeMatchup;
        public override Guid ComponentGuid => new Guid("50629678-376a-4f13-9da0-a3d560277341");
    }
}

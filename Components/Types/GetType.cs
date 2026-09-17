using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up one elemental type's damage relations against every other type.
    public class GetTypeComponent : GH_Component
    {
        public GetTypeComponent()
          : base("Get Type", "Type",
              "Looks up an elemental type's damage relations to and from every other type.",
              PluginUtilities.TabName, PluginUtilities.CategoryABTypes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Type Name", "TN", "Elemental type name (e.g. \"fire\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Double Damage To", "DT", "Types this type deals double damage to.", GH_ParamAccess.list);
            pManager.AddTextParameter("Double Damage From", "DF", "Types this type takes double damage from.", GH_ParamAccess.list);
            pManager.AddTextParameter("Half Damage To", "HT", "Types this type deals half damage to.", GH_ParamAccess.list);
            pManager.AddTextParameter("Half Damage From", "HF", "Types this type takes half damage from.", GH_ParamAccess.list);
            pManager.AddTextParameter("No Damage To", "NT", "Types this type deals no damage to.", GH_ParamAccess.list);
            pManager.AddTextParameter("No Damage From", "NF", "Types this type takes no damage from.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
            // extend-round-1: appended at the end so existing output indices (0-7) are unchanged
            pManager.AddTextParameter("Associated Pokemon", "AP", "Every Pokemon that has this type.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string typeName = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref typeName);

            // 3. Guard clauses / clamping
            if (string.IsNullOrWhiteSpace(typeName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Type Name is empty.");
                DA.SetData(6, "Error: Type Name is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetTypeAsync(typeName)).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(6, "Error: " + err);
                return;
            }

            JObject parsed = null;
            try { parsed = JObject.Parse(json); }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }

            var doubleTo = new List<string>();
            var doubleFrom = new List<string>();
            var halfTo = new List<string>();
            var halfFrom = new List<string>();
            var noTo = new List<string>();
            var noFrom = new List<string>();
            var associatedPokemon = new List<string>();

            if (parsed != null)
            {
                var relations = parsed["damage_relations"];
                FillNames(relations?["double_damage_to"] as JArray, doubleTo);
                FillNames(relations?["double_damage_from"] as JArray, doubleFrom);
                FillNames(relations?["half_damage_to"] as JArray, halfTo);
                FillNames(relations?["half_damage_from"] as JArray, halfFrom);
                FillNames(relations?["no_damage_to"] as JArray, noTo);
                FillNames(relations?["no_damage_from"] as JArray, noFrom);

                associatedPokemon.AddRange(PokeDataParsing.AssociatedPokemonNames(parsed["pokemon"] as JArray));
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, doubleTo);
            DA.SetDataList(1, doubleFrom);
            DA.SetDataList(2, halfTo);
            DA.SetDataList(3, halfFrom);
            DA.SetDataList(4, noTo);
            DA.SetDataList(5, noFrom);
            DA.SetData(6, "OK");
            DA.SetData(7, json);
            DA.SetDataList(8, associatedPokemon);
        }

        private static void FillNames(JArray array, List<string> into)
        {
            if (array == null) return;
            foreach (var item in array)
                into.Add((string)item["name"] ?? "");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetType;
        public override Guid ComponentGuid => new Guid("5a9b03e5-6840-4e1b-984f-e1d3742ee7d8");
    }
}

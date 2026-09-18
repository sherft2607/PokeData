using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up one elemental type's outgoing damage matchups against every other type — a
    // focused counterpart to Get Type's full to/from relation set, for "what should I use
    // against X" workflows.
    public class TypeMatchupComponent : GH_Component
    {
        public TypeMatchupComponent()
          : base("Type Matchup", "Matchup",
              "Looks up which types an elemental type deals double, half, or no damage to.",
              PluginUtilities.TabName, PluginUtilities.CategoryABTypes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Attacking Type", "AT", "Elemental type name (e.g. \"fire\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("2x Damage To", "DD", "Types this type deals double damage to.", GH_ParamAccess.list);
            pManager.AddTextParameter("0.5x Damage To", "HD", "Types this type deals half damage to.", GH_ParamAccess.list);
            pManager.AddTextParameter("0x Damage To", "ND", "Types this type deals no damage to.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Attacking Type is empty.");
                DA.SetData(3, "Error: Attacking Type is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetTypeAsync(typeName)).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(3, "Error: " + err);
                return;
            }

            JObject parsed = null;
            try { parsed = JObject.Parse(json); }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }

            List<string> doubleTo = new List<string>();
            List<string> halfTo = new List<string>();
            List<string> noTo = new List<string>();

            if (parsed != null)
            {
                doubleTo = PokeDataParsing.NamesToList(PokeDataParsing.SafeArray(parsed, "damage_relations", "double_damage_to"));
                halfTo = PokeDataParsing.NamesToList(PokeDataParsing.SafeArray(parsed, "damage_relations", "half_damage_to"));
                noTo = PokeDataParsing.NamesToList(PokeDataParsing.SafeArray(parsed, "damage_relations", "no_damage_to"));
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, doubleTo);
            DA.SetDataList(1, halfTo);
            DA.SetDataList(2, noTo);
            DA.SetData(3, "OK");
            DA.SetData(4, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_TypeMatchup;
        public override Guid ComponentGuid => new Guid("add5b998-d3b5-4d80-93ae-7e4deec76951");
    }
}

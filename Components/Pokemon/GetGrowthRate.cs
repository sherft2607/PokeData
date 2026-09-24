using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a growth rate's formula and max level by name or ID. Growth Rate Name Or ID
    // matches Get Pokemon Species' Growth Rate output exactly, so it chains straight in.
    public class GetGrowthRateComponent : GH_Component
    {
        public GetGrowthRateComponent()
          : base("Get Growth Rate", "GrowthRate",
              "Looks up a growth rate's experience formula.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAPokemon)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Growth Rate Name Or ID", "GR", "Growth rate name or numeric ID (e.g. \"slow\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Formula", "F", "LaTeX experience formula.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Max Level", "ML", "Number of levels this growth rate defines (typically 100).", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string nameOrId = "";
            DA.GetData(0, ref nameOrId);

            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Growth Rate Name Or ID is empty.");
                DA.SetData(2, "Error: Growth Rate Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetGrowthRateAsync(nameOrId)).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(2, "Error: " + err);
                return;
            }

            JObject parsed = null;
            try { parsed = JObject.Parse(json); }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }

            string formula = "";
            int maxLevel = 0;

            if (parsed != null)
            {
                formula = PokeDataParsing.SafeString(parsed, "formula");
                var levels = PokeDataParsing.SafeArray(parsed, "levels");
                maxLevel = levels?.Count ?? 0;
            }

            DA.SetData(0, formula);
            DA.SetData(1, maxLevel);
            DA.SetData(2, "OK");
            DA.SetData(3, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetGrowthRate;
        public override Guid ComponentGuid => new Guid("0c3d548f-6879-4281-3402-34d8596071c2");
    }
}

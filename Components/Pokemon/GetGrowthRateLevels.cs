using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Growth rate level/experience curve, split out from Get Growth Rate so the curve can feed
    // a plot directly. Reuses growth-rate_read — the shared response cache means calling this
    // alongside Get Growth Rate for the same rate only fetches once.
    public class GetGrowthRateLevelsComponent : GH_Component
    {
        public GetGrowthRateLevelsComponent()
          : base("Get Growth Rate Levels", "GrowthRateLevels",
              "Looks up a growth rate's full level-by-level experience curve.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAPokemon)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Growth Rate Name Or ID", "GR", "Growth rate name or numeric ID (e.g. \"slow\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Levels", "L", "Level numbers, parallel to Experience.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Experience", "E", "Total experience required per level, parallel to Levels.", GH_ParamAccess.list);
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

            var levels = new List<int>();
            var experience = new List<int>();

            if (parsed != null)
            {
                PokeDataParsing.ParseGrowthRateLevels(PokeDataParsing.SafeArray(parsed, "levels"), levels, experience);
            }

            DA.SetDataList(0, levels);
            DA.SetDataList(1, experience);
            DA.SetData(2, "OK");
            DA.SetData(3, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetGrowthRateLevels;
        public override Guid ComponentGuid => new Guid("1d4e659a-7980-4392-4513-45e9607182d3");
    }
}

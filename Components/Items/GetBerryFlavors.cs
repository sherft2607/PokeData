using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Berry flavor breakdown, split out from Get Berry so the flavor/potency pair can feed
    // list-based tools (e.g. Pokemon Filter) directly. Reuses berry_read — same endpoint as
    // Get Berry, called independently (not cached across components on the canvas, but each
    // component only calls it once per solve regardless of how many outputs it uses).
    public class GetBerryFlavorsComponent : GH_Component
    {
        public GetBerryFlavorsComponent()
          : base("Get Berry Flavors", "BerryFlavors",
              "Looks up a berry's flavor potencies (spicy/dry/sweet/bitter/sour).",
              PluginUtilities.TabName, PluginUtilities.CategoryAEItems)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Berry Name Or ID", "B", "Berry name or numeric ID (e.g. \"cheri\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Flavor Names", "FN", "Flavor names, parallel to Potencies.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Potencies", "P", "Potency per flavor (0 = not present), parallel to Flavor Names.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string nameOrId = "";
            DA.GetData(0, ref nameOrId);

            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Berry Name Or ID is empty.");
                DA.SetData(2, "Error: Berry Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetBerryAsync(nameOrId)).GetAwaiter().GetResult();

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

            var flavorNames = new List<string>();
            var potencies = new List<int>();

            if (parsed != null)
            {
                PokeDataParsing.ParseBerryFlavors(PokeDataParsing.SafeArray(parsed, "flavors"), flavorNames, potencies);
            }

            DA.SetDataList(0, flavorNames);
            DA.SetDataList(1, potencies);
            DA.SetData(2, "OK");
            DA.SetData(3, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetBerryFlavors;
        public override Guid ComponentGuid => new Guid("c2f3b4d5-6e7f-4081-9a02-3c4d5e6f7182");
    }
}

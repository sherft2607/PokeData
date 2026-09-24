using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Location area breakdown, split out from Get Location so the area list can feed list-based
    // tools without cluttering the core location lookup. Reuses location_read — same endpoint as
    // Get Location. Only the area name is exposed (no separate location-area fetch).
    public class GetLocationAreasComponent : GH_Component
    {
        public GetLocationAreasComponent()
          : base("Get Location Areas", "LocationAreas",
              "Looks up a location's sub-area names.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGData)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Location Name Or ID", "L", "Location name or numeric ID (e.g. \"canalave-city\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Area Names", "AN", "Names of every sub-area within this location.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string nameOrId = "";
            DA.GetData(0, ref nameOrId);

            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Location Name Or ID is empty.");
                DA.SetData(1, "Error: Location Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetLocationAsync(nameOrId)).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(1, "Error: " + err);
                return;
            }

            JObject parsed = null;
            try { parsed = JObject.Parse(json); }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }

            List<string> areaNames = new List<string>();

            if (parsed != null)
            {
                areaNames = PokeDataParsing.NamesToList(PokeDataParsing.SafeArray(parsed, "areas"));
            }

            DA.SetDataList(0, areaNames);
            DA.SetData(1, "OK");
            DA.SetData(2, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetLocationAreas;
        public override Guid ComponentGuid => new Guid("e4b5d6f7-8091-4a03-bc24-5e6f71829304");
    }
}

using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Region location breakdown, split out from Get Region so the location list can feed
    // list-based tools. Reuses region_read — the shared response cache means calling this
    // alongside Get Region / Get Region Pokedexes for the same region only fetches once.
    public class GetRegionLocationsComponent : GH_Component
    {
        public GetRegionLocationsComponent()
          : base("Get Region Locations", "RegionLocations",
              "Looks up a region's location names.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGData)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Region Name Or ID", "RG", "Region name or numeric ID (e.g. \"kanto\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Location Names", "LN", "Names of every location within this region — feeds Get Location per entry.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string nameOrId = "";
            DA.GetData(0, ref nameOrId);

            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Region Name Or ID is empty.");
                DA.SetData(1, "Error: Region Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetRegionAsync(nameOrId)).GetAwaiter().GetResult();

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

            List<string> locationNames = new List<string>();

            if (parsed != null)
            {
                locationNames = PokeDataParsing.NamesToList(PokeDataParsing.SafeArray(parsed, "locations"));
            }

            DA.SetDataList(0, locationNames);
            DA.SetData(1, "OK");
            DA.SetData(2, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetRegionLocations;
        public override Guid ComponentGuid => new Guid("b7e8093a-1324-4d36-ef57-8f9304152637");
    }
}

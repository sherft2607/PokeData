using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Region pokedex breakdown, split out from Get Region so the pokedex list can feed
    // list-based tools. Reuses region_read — the shared response cache means calling this
    // alongside Get Region / Get Region Locations for the same region only fetches once.
    // Pokedex Name Or ID matches Get Pokedex's input exactly, so it chains straight in.
    public class GetRegionPokedexesComponent : GH_Component
    {
        public GetRegionPokedexesComponent()
          : base("Get Region Pokedexes", "RegionPokedexes",
              "Looks up a region's pokedex names.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGData)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Region Name Or ID", "RG", "Region name or numeric ID (e.g. \"kanto\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Pokedex Name Or ID", "PX", "Names of every pokedex covering this region — feeds Get Pokedex per entry.", GH_ParamAccess.list);
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

            List<string> pokedexNames = new List<string>();

            if (parsed != null)
            {
                pokedexNames = PokeDataParsing.NamesToList(PokeDataParsing.SafeArray(parsed, "pokedexes"));
            }

            DA.SetDataList(0, pokedexNames);
            DA.SetData(1, "OK");
            DA.SetData(2, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetRegionPokedexes;
        public override Guid ComponentGuid => new Guid("c8f9104b-2435-4e47-f068-90a415263748");
    }
}

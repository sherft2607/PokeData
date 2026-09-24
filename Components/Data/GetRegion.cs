using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a region's main generation by name or ID. Region Name Or ID matches Get Location's
    // Region output exactly, so it chains straight in.
    public class GetRegionComponent : GH_Component
    {
        public GetRegionComponent()
          : base("Get Region", "Region",
              "Looks up a region's name and main generation.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGData)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Region Name Or ID", "RG", "Region name or numeric ID (e.g. \"kanto\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Region name.", GH_ParamAccess.item);
            pManager.AddTextParameter("Main Generation", "MG", "The generation this region was introduced in.", GH_ParamAccess.item);
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
                DA.SetData(2, "Error: Region Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetRegionAsync(nameOrId)).GetAwaiter().GetResult();

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

            string name = "", mainGeneration = "";

            if (parsed != null)
            {
                name = PokeDataParsing.SafeString(parsed, "name");
                mainGeneration = PokeDataParsing.SafeString(parsed, "main_generation", "name");
            }

            DA.SetData(0, name);
            DA.SetData(1, mainGeneration);
            DA.SetData(2, "OK");
            DA.SetData(3, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetRegion;
        public override Guid ComponentGuid => new Guid("a6d7f8b9-0213-4c25-de46-7f8293041526");
    }
}

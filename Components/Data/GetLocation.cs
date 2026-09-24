using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a location's region by name or ID.
    public class GetLocationComponent : GH_Component
    {
        public GetLocationComponent()
          : base("Get Location", "Location",
              "Looks up a location's name and region.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGData)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Location Name Or ID", "L", "Location name or numeric ID (e.g. \"canalave-city\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Location name.", GH_ParamAccess.item);
            pManager.AddTextParameter("Region", "RG", "Region this location belongs to.", GH_ParamAccess.item);
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
                DA.SetData(2, "Error: Location Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetLocationAsync(nameOrId)).GetAwaiter().GetResult();

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

            string name = "", region = "";

            if (parsed != null)
            {
                name = PokeDataParsing.SafeString(parsed, "name");
                region = PokeDataParsing.SafeString(parsed, "region", "name");
            }

            DA.SetData(0, name);
            DA.SetData(1, region);
            DA.SetData(2, "OK");
            DA.SetData(3, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetLocation;
        public override Guid ComponentGuid => new Guid("d3a4c5e6-7f80-4192-ab13-4d5e6f718293");
    }
}

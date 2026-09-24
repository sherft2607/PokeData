using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a pokedex's name and whether it's a main-series dex by name or ID. Pokedex Name
    // Or ID matches Get Region Pokedexes' output exactly, so it chains straight in.
    public class GetPokedexComponent : GH_Component
    {
        public GetPokedexComponent()
          : base("Get Pokedex", "Pokedex",
              "Looks up a pokedex's name and main-series flag.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGData)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pokedex Name Or ID", "PX", "Pokedex name or numeric ID (e.g. \"kanto\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Pokedex name.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Is Main Series", "IM", "Whether this is a main-series Pokedex.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string nameOrId = "";
            DA.GetData(0, ref nameOrId);

            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Pokedex Name Or ID is empty.");
                DA.SetData(2, "Error: Pokedex Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetPokedexAsync(nameOrId)).GetAwaiter().GetResult();

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

            string name = "";
            bool isMainSeries = false;

            if (parsed != null)
            {
                name = PokeDataParsing.SafeString(parsed, "name");
                isMainSeries = (bool?)PokeDataParsing.SafeChild(parsed, "is_main_series") ?? false;
            }

            DA.SetData(0, name);
            DA.SetData(1, isMainSeries);
            DA.SetData(2, "OK");
            DA.SetData(3, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetPokedex;
        public override Guid ComponentGuid => new Guid("d90a215c-3546-4f58-0179-01a526374859");
    }
}

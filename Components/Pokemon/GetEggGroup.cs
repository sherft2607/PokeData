using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up an egg group's member species by name or ID. Egg Group Name Or ID matches one
    // entry of Get Pokemon Species' Egg Groups list output, so it chains straight in per-item.
    // Small enough (one scalar + one list) that it isn't split like Berry/Location/Region/Pokedex.
    public class GetEggGroupComponent : GH_Component
    {
        public GetEggGroupComponent()
          : base("Get Egg Group", "EggGroup",
              "Looks up an egg group's member species.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAPokemon)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Egg Group Name Or ID", "EG", "Egg group name or numeric ID (e.g. \"monster\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Egg group name.", GH_ParamAccess.item);
            pManager.AddTextParameter("Species Names", "SN", "Every species belonging to this egg group.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string nameOrId = "";
            DA.GetData(0, ref nameOrId);

            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Egg Group Name Or ID is empty.");
                DA.SetData(2, "Error: Egg Group Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetEggGroupAsync(nameOrId)).GetAwaiter().GetResult();

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
            List<string> speciesNames = new List<string>();

            if (parsed != null)
            {
                name = PokeDataParsing.SafeString(parsed, "name");
                speciesNames = PokeDataParsing.NamesToList(PokeDataParsing.SafeArray(parsed, "pokemon_species"));
            }

            DA.SetData(0, name);
            DA.SetDataList(1, speciesNames);
            DA.SetData(2, "OK");
            DA.SetData(3, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetEggGroup;
        public override Guid ComponentGuid => new Guid("fb2c437e-5768-4170-2391-23c7485960b1");
    }
}

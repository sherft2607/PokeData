using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Pokedex species roster, split out from Get Pokedex so the species list can feed
    // list-based tools (same shape as Get Generation's Species Names). Reuses pokedex_read — the
    // shared response cache means calling this alongside Get Pokedex for the same dex only
    // fetches once.
    public class GetPokedexSpeciesComponent : GH_Component
    {
        public GetPokedexSpeciesComponent()
          : base("Get Pokedex Species", "PokedexSpecies",
              "Looks up every species entry in a pokedex, in dex-number order.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGData)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pokedex Name Or ID", "PX", "Pokedex name or numeric ID (e.g. \"kanto\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Species Names", "SN", "Species names in dex-entry order — feeds Get Pokemon Species / Pokemon Filter.", GH_ParamAccess.list);
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
                DA.SetData(1, "Error: Pokedex Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetPokedexAsync(nameOrId)).GetAwaiter().GetResult();

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

            List<string> speciesNames = new List<string>();

            if (parsed != null)
            {
                speciesNames = PokeDataParsing.PokedexSpeciesNames(PokeDataParsing.SafeArray(parsed, "pokemon_entries"));
            }

            DA.SetDataList(0, speciesNames);
            DA.SetData(1, "OK");
            DA.SetData(2, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetPokedexSpecies;
        public override Guid ComponentGuid => new Guid("ea1b326d-4657-4069-1280-12b6374859a0");
    }
}

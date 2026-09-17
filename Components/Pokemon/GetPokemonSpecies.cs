using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a Pokemon species' flavor/classification detail: color, shape, habitat, capture
    // rate, base happiness, growth rate, egg groups, and legendary/mythical flags.
    public class GetPokemonSpeciesComponent : GH_Component
    {
        public GetPokemonSpeciesComponent()
          : base("Get Pokemon Species", "Species",
              "Looks up a Pokemon species' color, shape, habitat, capture rate, growth rate, egg groups, and legendary/mythical flags.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAPokemon)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Species Name Or ID", "SP", "Pokemon species name or numeric ID (e.g. \"bulbasaur\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Color", "CO", "Pokedex color category.", GH_ParamAccess.item);
            pManager.AddTextParameter("Shape", "SH", "Body shape category.", GH_ParamAccess.item);
            pManager.AddTextParameter("Habitat", "HA", "Habitat category (empty for species with no habitat, e.g. many legendaries).", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Capture Rate", "CR", "Base capture rate (0-255, higher is easier).", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Base Happiness", "BH", "Base friendship value.", GH_ParamAccess.item);
            pManager.AddTextParameter("Growth Rate", "GR", "Leveling growth rate category.", GH_ParamAccess.item);
            pManager.AddTextParameter("Egg Groups", "EG", "Breeding egg groups.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Is Legendary", "IL", "True if this species is legendary.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Is Mythical", "IM", "True if this species is mythical.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string nameOrId = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref nameOrId);

            // 3. Guard clauses / clamping
            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Species Name Or ID is empty.");
                DA.SetData(9, "Error: Species Name Or ID is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetPokemonSpeciesAsync(nameOrId)).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(9, "Error: " + err);
                return;
            }

            JObject parsed = null;
            try { parsed = JObject.Parse(json); }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }

            string color = "", shape = "", habitat = "", growthRate = "";
            int captureRate = 0, baseHappiness = 0;
            var eggGroups = new List<string>();
            bool isLegendary = false, isMythical = false;

            if (parsed != null)
            {
                color = (string)parsed["color"]?["name"] ?? "";
                shape = (string)parsed["shape"]?["name"] ?? "";
                habitat = (string)parsed["habitat"]?["name"] ?? "";
                captureRate = (int?)parsed["capture_rate"] ?? 0;
                baseHappiness = (int?)parsed["base_happiness"] ?? 0;
                growthRate = (string)parsed["growth_rate"]?["name"] ?? "";
                isLegendary = (bool?)parsed["is_legendary"] ?? false;
                isMythical = (bool?)parsed["is_mythical"] ?? false;

                eggGroups.AddRange(PokeDataParsing.NamesToList(parsed["egg_groups"] as JArray));
            }

            // 5. DA.SetData calls
            DA.SetData(0, color);
            DA.SetData(1, shape);
            DA.SetData(2, habitat);
            DA.SetData(3, captureRate);
            DA.SetData(4, baseHappiness);
            DA.SetData(5, growthRate);
            DA.SetDataList(6, eggGroups);
            DA.SetData(7, isLegendary);
            DA.SetData(8, isMythical);
            DA.SetData(9, "OK");
            DA.SetData(10, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetPokemonSpecies;
        public override Guid ComponentGuid => new Guid("dcb154a8-c67c-404e-a7e9-4efe99a242af");
    }
}

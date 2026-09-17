using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a single Pokemon by name or ID.
    // Outputs types, height, weight, base stats, and both sprite image URLs for use with image samplers.
    public class GetPokemonComponent : GH_Component
    {
        public GetPokemonComponent()
          : base("Get Pokemon", "Poke",
              "Looks up a Pokemon by name or ID and returns its types, size, stats, and sprite URLs.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAPokemon)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pokemon Name Or ID", "P", "Pokemon name or numeric ID (e.g. \"pikachu\" or 25).", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Types", "TY", "Elemental types.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height", "H", "Height in decimetres.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight", "W", "Weight in hectograms.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Base Experience", "BE", "Base experience yield.", GH_ParamAccess.item);
            pManager.AddTextParameter("Stat Names", "SN", "Base stat names, in order: hp, attack, defense, special-attack, special-defense, speed.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Stat Values", "SV", "Base stat values, parallel to Stat Names.", GH_ParamAccess.list);
            pManager.AddTextParameter("Sprite Front URL", "SF", "Front-facing default sprite image URL.", GH_ParamAccess.item);
            pManager.AddTextParameter("Sprite Artwork URL", "SA", "Official artwork image URL.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
            // extend-round-1: appended at the end so existing output indices (0-9) are unchanged
            pManager.AddTextParameter("Legacy Cry URL", "LC", "Legacy (pre-Gen IX) cry audio URL.", GH_ParamAccess.item);
            pManager.AddTextParameter("Latest Cry URL", "LT", "Latest cry audio URL.", GH_ParamAccess.item);
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Pokemon Name Or ID is empty.");
                DA.SetData(8, "Error: Pokemon Name Or ID is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetPokemonAsync(nameOrId)).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(8, "Error: " + err);
                return;
            }

            JObject parsed = null;
            try { parsed = JObject.Parse(json); }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }

            var types = new List<string>();
            var statNames = new List<string>();
            var statValues = new List<int>();
            double height = 0, weight = 0;
            int baseExperience = 0;
            string spriteFront = "";
            string spriteArtwork = "";
            string legacyCry = "";
            string latestCry = "";

            if (parsed != null)
            {
                var typesArray = parsed["types"] as JArray;
                if (typesArray != null)
                {
                    foreach (var t in typesArray)
                        types.Add((string)t["type"]?["name"] ?? "");
                }

                height = (double?)parsed["height"] ?? 0;
                weight = (double?)parsed["weight"] ?? 0;
                baseExperience = (int?)parsed["base_experience"] ?? 0;

                PokeDataParsing.ParseStats(parsed["stats"] as JArray, statNames, statValues);

                spriteFront = (string)parsed["sprites"]?["front_default"] ?? "";
                spriteArtwork = (string)parsed["sprites"]?["other"]?["official-artwork"]?["front_default"] ?? "";
                PokeDataParsing.ParseCries(parsed, out legacyCry, out latestCry);
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, types);
            DA.SetData(1, height);
            DA.SetData(2, weight);
            DA.SetData(3, baseExperience);
            DA.SetDataList(4, statNames);
            DA.SetDataList(5, statValues);
            DA.SetData(6, spriteFront);
            DA.SetData(7, spriteArtwork);
            DA.SetData(8, "OK");
            DA.SetData(9, json);
            DA.SetData(10, legacyCry);
            DA.SetData(11, latestCry);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetPokemon;
        public override Guid ComponentGuid => new Guid("c9b5a94b-fa2b-4449-a317-715d040be011");
    }
}

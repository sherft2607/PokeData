using Grasshopper.Kernel;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a Pokemon's cry audio URLs and, when triggered, streams the audio bytes in-memory
    // and hands them off to the OS's default player. Cries are hosted on
    // raw.githubusercontent.com, not pokeapi.co, but the URLs themselves come from the pokemon
    // detail response's "cries" field (same field Get Pokemon already exposes).
    public class PokemonCryComponent : GH_Component
    {
        public PokemonCryComponent()
          : base("Pokemon Cry", "Cry",
              "Looks up a Pokemon's cry audio URLs and optionally plays the cry when triggered.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAPokemon)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pokemon Name Or ID", "P", "Pokemon name or numeric ID (e.g. \"pikachu\" or 25).", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Play", "PL", "Set true to download and play the Latest Cry audio.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Legacy Cry URL", "LC", "Legacy (pre-Gen IX) cry audio URL.", GH_ParamAccess.item);
            pManager.AddTextParameter("Latest Cry URL", "LT", "Latest cry audio URL.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string nameOrId = "";
            bool play = false;

            // 2. DA.GetData calls
            DA.GetData(0, ref nameOrId);
            DA.GetData(1, ref play);

            // 3. Guard clauses / clamping
            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Pokemon Name Or ID is empty.");
                DA.SetData(2, "Error: Pokemon Name Or ID is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetPokemonAsync(nameOrId)).GetAwaiter().GetResult();

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

            string legacyCry = "", latestCry = "";
            if (parsed != null)
                PokeDataParsing.ParseCries(parsed, out legacyCry, out latestCry);

            // 5. DA.SetData calls
            DA.SetData(0, legacyCry);
            DA.SetData(1, latestCry);
            DA.SetData(2, "OK");

            if (!play) return;

            string urlToPlay = !string.IsNullOrWhiteSpace(latestCry) ? latestCry : legacyCry;
            if (string.IsNullOrWhiteSpace(urlToPlay))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No cry URL available to play.");
                return;
            }

            var (bytesOk, bytes, bytesErr) = Task.Run(() => client.GetImageBytesAsync(urlToPlay)).GetAwaiter().GetResult();
            if (!bytesOk)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not download cry audio: " + bytesErr);
                return;
            }

            try
            {
                // Downloaded fully in-memory above; the only disk touch is this transient handoff
                // file for the OS's own player (no in-process .ogg decoder is referenced by this
                // plugin) — nothing here is read back or locked by this process afterward.
                string tempFile = Path.Combine(Path.GetTempPath(), "pokedata_cry_" + Guid.NewGuid().ToString("N") + ".ogg");
                File.WriteAllBytes(tempFile, bytes);
                Process.Start(new ProcessStartInfo(tempFile) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not play cry audio: " + ex.Message);
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_PokemonCry;
        public override Guid ComponentGuid => new Guid("fc446246-4ee3-4722-840d-3d478df3cc35");
    }
}

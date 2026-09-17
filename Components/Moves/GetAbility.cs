using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up an ability's generation and effect text by name or ID.
    public class GetAbilityComponent : GH_Component
    {
        public GetAbilityComponent()
          : base("Get Ability", "Ability",
              "Looks up an ability's introducing generation and effect text.",
              PluginUtilities.TabName, PluginUtilities.CategoryADMoves)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Ability Name Or ID", "A", "Ability name or numeric ID (e.g. \"static\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Generation", "G", "Generation this ability was introduced in.", GH_ParamAccess.item);
            pManager.AddTextParameter("Effect Text", "E", "Short English effect description.", GH_ParamAccess.item);
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Ability Name Or ID is empty.");
                DA.SetData(2, "Error: Ability Name Or ID is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetAbilityAsync(nameOrId)).GetAwaiter().GetResult();

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

            string generation = "", effectText = "";

            if (parsed != null)
            {
                generation = (string)parsed["generation"]?["name"] ?? "";

                var effectEntries = parsed["effect_entries"] as JArray;
                if (effectEntries != null)
                {
                    foreach (var entry in effectEntries)
                    {
                        if ((string)entry["language"]?["name"] == "en")
                        {
                            effectText = (string)entry["effect"] ?? "";
                            break;
                        }
                    }
                }
            }

            // 5. DA.SetData calls
            DA.SetData(0, generation);
            DA.SetData(1, effectText);
            DA.SetData(2, "OK");
            DA.SetData(3, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetAbility;
        public override Guid ComponentGuid => new Guid("be5ee826-8540-4da9-9377-0535e4df7047");
    }
}

using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up an item's category, cost, fling power, effect text, and sprite URL by name or ID.
    public class GetItemComponent : GH_Component
    {
        public GetItemComponent()
          : base("Get Item", "Item",
              "Looks up an item's category, cost, fling power, effect text, and sprite URL.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEItems)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Item Name Or ID", "I", "Item name or numeric ID (e.g. \"poke-ball\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Category", "C", "Item category.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Cost", "CO", "In-game purchase cost, in Pokedollars.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Fling Power", "FP", "Base power when used with the move Fling (0 if not flingable).", GH_ParamAccess.item);
            pManager.AddTextParameter("Effect Text", "E", "Short English effect description.", GH_ParamAccess.item);
            pManager.AddTextParameter("Sprite URL", "SP", "Item sprite image URL.", GH_ParamAccess.item);
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Item Name Or ID is empty.");
                DA.SetData(5, "Error: Item Name Or ID is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetItemAsync(nameOrId)).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(5, "Error: " + err);
                return;
            }

            JObject parsed = null;
            try { parsed = JObject.Parse(json); }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }

            string category = "", effectText = "", spriteUrl = "";
            int cost = 0, flingPower = 0;

            if (parsed != null)
            {
                category = PokeDataParsing.SafeString(parsed, "category", "name");
                cost = (int?)parsed["cost"] ?? 0;
                flingPower = (int?)parsed["fling_power"] ?? 0;
                spriteUrl = PokeDataParsing.SafeString(parsed, "sprites", "default");

                var effectEntries = parsed["effect_entries"] as JArray;
                if (effectEntries != null)
                {
                    foreach (var entry in effectEntries)
                    {
                        if (PokeDataParsing.SafeString(entry, "language", "name") == "en")
                        {
                            effectText = PokeDataParsing.SafeString(entry, "effect");
                            break;
                        }
                    }
                }
            }

            // 5. DA.SetData calls
            DA.SetData(0, category);
            DA.SetData(1, cost);
            DA.SetData(2, flingPower);
            DA.SetData(3, effectText);
            DA.SetData(4, spriteUrl);
            DA.SetData(5, "OK");
            DA.SetData(6, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetItem;
        public override Guid ComponentGuid => new Guid("f6d8e9f0-94d0-4e7f-9bc0-49e8e87cf4c6");
    }
}

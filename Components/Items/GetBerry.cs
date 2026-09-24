using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a berry's growth/size/gift stats by name or ID. Every berry is also an item, so
    // Item Name Or ID is named/typed identically to GetItemComponent's input for direct chaining.
    public class GetBerryComponent : GH_Component
    {
        public GetBerryComponent()
          : base("Get Berry", "Berry",
              "Looks up a berry's growth time, harvest, size, and natural gift stats.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEItems)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Berry Name Or ID", "B", "Berry name or numeric ID (e.g. \"cheri\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Berry name.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Growth Time", "GT", "Hours per growth stage.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Max Harvest", "MH", "Maximum number harvested from one berry tree.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Size", "SZ", "Berry size, in millimeters.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Smoothness", "SM", "Smoothness (affects Pokeblock/Poffin quality).", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Soil Dryness", "SD", "Soil drying rate while planted.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Natural Gift Power", "NP", "Power when used as the move Natural Gift.", GH_ParamAccess.item);
            pManager.AddTextParameter("Natural Gift Type", "NG", "Elemental type when used as Natural Gift.", GH_ParamAccess.item);
            pManager.AddTextParameter("Item Name Or ID", "I", "The item this berry corresponds to — wire directly into Get Item.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string nameOrId = "";
            DA.GetData(0, ref nameOrId);

            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Berry Name Or ID is empty.");
                DA.SetData(9, "Error: Berry Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetBerryAsync(nameOrId)).GetAwaiter().GetResult();

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

            string name = "", naturalGiftType = "", itemNameOrId = "";
            int growthTime = 0, maxHarvest = 0, size = 0, smoothness = 0, soilDryness = 0, naturalGiftPower = 0;

            if (parsed != null)
            {
                name = PokeDataParsing.SafeString(parsed, "name");
                growthTime = (int?)PokeDataParsing.SafeChild(parsed, "growth_time") ?? 0;
                maxHarvest = (int?)PokeDataParsing.SafeChild(parsed, "max_harvest") ?? 0;
                size = (int?)PokeDataParsing.SafeChild(parsed, "size") ?? 0;
                smoothness = (int?)PokeDataParsing.SafeChild(parsed, "smoothness") ?? 0;
                soilDryness = (int?)PokeDataParsing.SafeChild(parsed, "soil_dryness") ?? 0;
                naturalGiftPower = (int?)PokeDataParsing.SafeChild(parsed, "natural_gift_power") ?? 0;
                naturalGiftType = PokeDataParsing.SafeString(parsed, "natural_gift_type", "name");
                itemNameOrId = PokeDataParsing.SafeString(parsed, "item", "name");
            }

            DA.SetData(0, name);
            DA.SetData(1, growthTime);
            DA.SetData(2, maxHarvest);
            DA.SetData(3, size);
            DA.SetData(4, smoothness);
            DA.SetData(5, soilDryness);
            DA.SetData(6, naturalGiftPower);
            DA.SetData(7, naturalGiftType);
            DA.SetData(8, itemNameOrId);
            DA.SetData(9, "OK");
            DA.SetData(10, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetBerry;
        public override Guid ComponentGuid => new Guid("b1e2a3c4-5d6e-4f70-8a91-2b3c4d5e6f71");
    }
}

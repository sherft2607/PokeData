using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up whether a stat is battle-only by name or ID. Stat Name Or ID matches Get Nature's
    // Increased Stat / Decreased Stat outputs and Get Pokemon's Stat Names list entries, so it
    // chains straight in.
    public class GetStatComponent : GH_Component
    {
        public GetStatComponent()
          : base("Get Stat", "Stat",
              "Looks up whether a stat only applies during battle.",
              PluginUtilities.TabName, PluginUtilities.CategoryAFStats)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Stat Name Or ID", "ST", "Stat name or numeric ID (e.g. \"attack\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Is Battle Only", "IB", "Whether this stat only exists during battle (e.g. accuracy/evasion).", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string nameOrId = "";
            DA.GetData(0, ref nameOrId);

            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Stat Name Or ID is empty.");
                DA.SetData(1, "Error: Stat Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetStatAsync(nameOrId)).GetAwaiter().GetResult();

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

            bool isBattleOnly = false;

            if (parsed != null)
            {
                isBattleOnly = (bool?)PokeDataParsing.SafeChild(parsed, "is_battle_only") ?? false;
            }

            DA.SetData(0, isBattleOnly);
            DA.SetData(1, "OK");
            DA.SetData(2, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetStat;
        public override Guid ComponentGuid => new Guid("2e5f76ab-8a91-4403-5624-56fa718293e4");
    }
}

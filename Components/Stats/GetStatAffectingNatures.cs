using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Reverse lookup of Get Nature: which natures increase or decrease a given stat. Split out
    // from Get Stat so the nature lists can feed list-based tools. Reuses stat_read — the shared
    // response cache means calling this alongside Get Stat for the same stat only fetches once.
    public class GetStatAffectingNaturesComponent : GH_Component
    {
        public GetStatAffectingNaturesComponent()
          : base("Get Stat Affecting Natures", "StatNatures",
              "Looks up which natures increase or decrease a stat.",
              PluginUtilities.TabName, PluginUtilities.CategoryAFStats)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Stat Name Or ID", "ST", "Stat name or numeric ID (e.g. \"attack\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Increasing Natures", "IN", "Natures that raise this stat by 10% — feeds Get Nature per entry.", GH_ParamAccess.list);
            pManager.AddTextParameter("Decreasing Natures", "DN", "Natures that lower this stat by 10% — feeds Get Nature per entry.", GH_ParamAccess.list);
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
                DA.SetData(2, "Error: Stat Name Or ID is empty.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetStatAsync(nameOrId)).GetAwaiter().GetResult();

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

            var increasing = new List<string>();
            var decreasing = new List<string>();

            if (parsed != null)
            {
                PokeDataParsing.ParseStatAffectingNatures(parsed, increasing, decreasing);
            }

            DA.SetDataList(0, increasing);
            DA.SetDataList(1, decreasing);
            DA.SetData(2, "OK");
            DA.SetData(3, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetStatAffectingNatures;
        public override Guid ComponentGuid => new Guid("3f6087bc-9ba2-4514-6735-67ab8293f4d5");
    }
}

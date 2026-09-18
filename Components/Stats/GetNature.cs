using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a nature's stat modifiers and berry-flavor preferences by name or ID. Neutral
    // natures (e.g. "hardy") have no increased/decreased stat or liked/disliked flavor — those
    // outputs come back empty rather than erroring.
    public class GetNatureComponent : GH_Component
    {
        public GetNatureComponent()
          : base("Get Nature", "Nature",
              "Looks up a nature's +10%/-10% stat modifiers and liked/disliked berry flavors.",
              PluginUtilities.TabName, PluginUtilities.CategoryAFStats)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Nature Name Or ID", "N", "Nature name or numeric ID (e.g. \"adamant\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Increased Stat", "IS", "Stat raised by 10% (empty for a neutral nature).", GH_ParamAccess.item);
            pManager.AddTextParameter("Decreased Stat", "DS", "Stat lowered by 10% (empty for a neutral nature).", GH_ParamAccess.item);
            pManager.AddTextParameter("Liked Flavor", "LF", "Berry flavor this nature likes (empty for a neutral nature).", GH_ParamAccess.item);
            pManager.AddTextParameter("Disliked Flavor", "DF", "Berry flavor this nature dislikes (empty for a neutral nature).", GH_ParamAccess.item);
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Nature Name Or ID is empty.");
                DA.SetData(4, "Error: Nature Name Or ID is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetNatureAsync(nameOrId)).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(4, "Error: " + err);
                return;
            }

            JObject parsed = null;
            try { parsed = JObject.Parse(json); }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }

            string increasedStat = "", decreasedStat = "", likedFlavor = "", dislikedFlavor = "";

            if (parsed != null)
            {
                increasedStat = PokeDataParsing.SafeString(parsed, "increased_stat", "name");
                decreasedStat = PokeDataParsing.SafeString(parsed, "decreased_stat", "name");
                likedFlavor = PokeDataParsing.SafeString(parsed, "likes_flavor", "name");
                dislikedFlavor = PokeDataParsing.SafeString(parsed, "hates_flavor", "name");
            }

            // 5. DA.SetData calls
            DA.SetData(0, increasedStat);
            DA.SetData(1, decreasedStat);
            DA.SetData(2, likedFlavor);
            DA.SetData(3, dislikedFlavor);
            DA.SetData(4, "OK");
            DA.SetData(5, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetNature;
        public override Guid ComponentGuid => new Guid("54cee5bf-ce17-4ee0-9428-791f93cb7610");
    }
}

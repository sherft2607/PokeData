using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a generation (1-9) and returns its species roster and main region.
    public class GetGenerationComponent : GH_Component
    {
        public GetGenerationComponent()
          : base("Get Generation", "Gen",
              "Looks up a generation's species roster (IDs and names) and main region.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGData)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Generation", "G", "Generation number, 1-9.", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Species IDs", "SI", "Numeric species IDs introduced in this generation.", GH_ParamAccess.list);
            pManager.AddTextParameter("Species Names", "SN", "Species names introduced in this generation.", GH_ParamAccess.list);
            pManager.AddTextParameter("Region", "R", "This generation's main region.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "RS", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            int generation = 1;

            // 2. DA.GetData calls
            DA.GetData(0, ref generation);

            // 3. Guard clauses / clamping
            if (generation < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Generation must be 1 or greater.");
                DA.SetData(3, "Error: Generation must be 1 or greater.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetGenerationAsync(generation.ToString())).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(3, "Error: " + err);
                return;
            }

            JObject parsed = null;
            try { parsed = JObject.Parse(json); }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }

            var speciesIds = new System.Collections.Generic.List<int>();
            var speciesNames = new System.Collections.Generic.List<string>();
            string region = "";

            if (parsed != null)
            {
                var speciesArray = parsed["pokemon_species"] as JArray;
                speciesIds = PokeDataParsing.ExtractIdsFromUrls(speciesArray);
                speciesNames = PokeDataParsing.NamesToList(speciesArray);
                region = PokeDataParsing.SafeString(parsed, "main_region", "name");
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, speciesIds);
            DA.SetDataList(1, speciesNames);
            DA.SetData(2, region);
            DA.SetData(3, "OK");
            DA.SetData(4, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetGeneration;
        public override Guid ComponentGuid => new Guid("f4747cbb-0032-4241-977a-85c3eaaa37dc");
    }
}

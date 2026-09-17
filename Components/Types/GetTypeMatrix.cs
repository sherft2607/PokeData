using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Builds the full type-effectiveness matrix by listing every type, fetching each one's
    // damage relations, and flattening them into one 18x18 cross product. Consolidated so the
    // user does not have to wire 18 Get Type calls by hand.
    public class GetTypeMatrixComponent : GH_Component
    {
        public GetTypeMatrixComponent()
          : base("Get Type Matrix", "TypeMx",
              "Builds the full attacking/defending type-effectiveness matrix in one call.",
              PluginUtilities.TabName, PluginUtilities.CategoryABTypes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // no inputs — internally lists and fetches every type
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Attacking Type", "AT", "Attacking type name, one entry per matrix cell.", GH_ParamAccess.list);
            pManager.AddTextParameter("Defending Type", "DT", "Defending type name, one entry per matrix cell.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Multiplier", "M", "Damage multiplier (0, 0.5, 1, or 2), parallel to Attacking/Defending Type.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response of the type list call.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            // (no inputs to declare)

            // 2. DA.GetData calls
            // (none)

            // 3. Guard clauses / clamping
            // (none — this component takes no inputs)

            // 4. Logic
            var client = new PokeDataClient();
            var (listOk, listJson, listErr) = Task.Run(() => client.GetTypeListAsync()).GetAwaiter().GetResult();

            if (!listOk)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, listErr);
                DA.SetData(3, "Error: " + listErr);
                return;
            }

            var typeNames = new List<string>();
            try
            {
                var listParsed = JObject.Parse(listJson);
                var results = listParsed["results"] as JArray;
                if (results != null)
                    foreach (var r in results)
                        typeNames.Add((string)r["name"] ?? "");
            }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Type list response was not JSON.");
                DA.SetData(3, "Error: type list response was not JSON.");
                return;
            }

            var attacking = new List<string>();
            var defending = new List<string>();
            var multiplier = new List<double>();

            foreach (var typeName in typeNames)
            {
                var (ok, json, err) = Task.Run(() => client.GetTypeAsync(typeName)).GetAwaiter().GetResult();
                if (!ok)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, typeName + ": " + err);
                    continue;
                }

                JObject parsed;
                try { parsed = JObject.Parse(json); }
                catch (JsonException)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, typeName + ": response was not JSON.");
                    continue;
                }

                PokeDataParsing.FlattenTypeRow(typeName, parsed, typeNames, attacking, defending, multiplier);
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, attacking);
            DA.SetDataList(1, defending);
            DA.SetDataList(2, multiplier);
            DA.SetData(3, "OK");
            DA.SetData(4, listJson);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetTypeMatrix;
        public override Guid ComponentGuid => new Guid("9d050d91-e88a-4c8d-8a4a-8668f6c520b2");
    }
}

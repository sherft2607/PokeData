using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a TM/HM machine by its numeric ID (machine has no name field on PokeAPI — ID
    // only). Move Name Or ID and Item Name Or ID are named/typed identically to GetMove's and
    // GetItem's inputs, so a machine chains straight into either without a glue component.
    public class GetMachineComponent : GH_Component
    {
        public GetMachineComponent()
          : base("Get Machine", "Machine",
              "Looks up which move a TM/HM machine teaches and the item it corresponds to.",
              PluginUtilities.TabName, PluginUtilities.CategoryADMoves)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Machine ID", "MI", "Machine's numeric ID (machines have no name).", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Version Group", "VG", "Game version group this machine belongs to.", GH_ParamAccess.item);
            pManager.AddTextParameter("Move Name Or ID", "M", "The move this machine teaches — wire directly into Get Move.", GH_ParamAccess.item);
            pManager.AddTextParameter("Item Name Or ID", "I", "The TM/HM item — wire directly into Get Item.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int id = 0;
            DA.GetData(0, ref id);

            if (id <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Machine ID must be a positive integer.");
                DA.SetData(3, "Error: Machine ID must be a positive integer.");
                return;
            }

            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetMachineAsync(id)).GetAwaiter().GetResult();

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

            string versionGroup = "", moveNameOrId = "", itemNameOrId = "";

            if (parsed != null)
            {
                versionGroup = PokeDataParsing.SafeString(parsed, "version_group", "name");
                moveNameOrId = PokeDataParsing.SafeString(parsed, "move", "name");
                itemNameOrId = PokeDataParsing.SafeString(parsed, "item", "name");
            }

            DA.SetData(0, versionGroup);
            DA.SetData(1, moveNameOrId);
            DA.SetData(2, itemNameOrId);
            DA.SetData(3, "OK");
            DA.SetData(4, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetMachine;
        public override Guid ComponentGuid => new Guid("f5c6e7a8-9102-4b14-cd35-6f7182930415");
    }
}

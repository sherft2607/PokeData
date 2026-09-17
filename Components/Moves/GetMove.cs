using Grasshopper.Kernel;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a move's mechanical fields by name or ID.
    public class GetMoveComponent : GH_Component
    {
        public GetMoveComponent()
          : base("Get Move", "Move",
              "Looks up a move's power, accuracy, PP, damage class, type, and effect text.",
              PluginUtilities.TabName, PluginUtilities.CategoryADMoves)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Move Name Or ID", "M", "Move name or numeric ID (e.g. \"thunderbolt\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Power", "P", "Base power (may be empty for status moves).", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Accuracy", "A", "Accuracy percentage (may be empty for moves that always hit).", GH_ParamAccess.item);
            pManager.AddIntegerParameter("PP", "PP", "Power points.", GH_ParamAccess.item);
            pManager.AddTextParameter("Damage Class", "DC", "Physical, special, or status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "T", "Elemental type of the move.", GH_ParamAccess.item);
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Move Name Or ID is empty.");
                DA.SetData(6, "Error: Move Name Or ID is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, json, err) = Task.Run(() => client.GetMoveAsync(nameOrId)).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(6, "Error: " + err);
                return;
            }

            JObject parsed = null;
            try { parsed = JObject.Parse(json); }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Response was not JSON: " + (json.Length > 200 ? json.Substring(0, 200) + "…" : json));
            }

            int power = 0, accuracy = 0, pp = 0;
            string damageClass = "", type = "", effectText = "";

            if (parsed != null)
            {
                power = (int?)parsed["power"] ?? 0;
                accuracy = (int?)parsed["accuracy"] ?? 0;
                pp = (int?)parsed["pp"] ?? 0;
                damageClass = PokeDataParsing.SafeString(parsed, "damage_class", "name");
                type = PokeDataParsing.SafeString(parsed, "type", "name");

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
            DA.SetData(0, power);
            DA.SetData(1, accuracy);
            DA.SetData(2, pp);
            DA.SetData(3, damageClass);
            DA.SetData(4, type);
            DA.SetData(5, effectText);
            DA.SetData(6, "OK");
            DA.SetData(7, json);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetMove;
        public override Guid ComponentGuid => new Guid("1262cafd-ffcc-4762-93ed-dc9e6cd714ab");
    }
}

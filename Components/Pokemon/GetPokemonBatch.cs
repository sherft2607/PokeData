using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Looks up a list of Pokemon by name or ID, one branch per input item.
    // Same underlying endpoint as Get Pokemon, called once per list item.
    public class GetPokemonBatchComponent : GH_Component
    {
        public GetPokemonBatchComponent()
          : base("Get Pokemon Batch", "PokeB",
              "Looks up a list of Pokemon and returns their fields as a tree, one branch per input.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAPokemon)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pokemon Names Or IDs", "P", "List of Pokemon names or numeric IDs.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Types", "TY", "Elemental types, per Pokemon.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Height", "H", "Height in decimetres, per Pokemon.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Weight", "W", "Weight in hectograms, per Pokemon.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Base Experience", "BE", "Base experience yield, per Pokemon.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Stat Names", "SN", "Base stat names, per Pokemon.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Stat Values", "SV", "Base stat values, per Pokemon.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Sprite Front URL", "SF", "Front-facing default sprite URL, per Pokemon.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Sprite Artwork URL", "SA", "Official artwork URL, per Pokemon.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Status", "S", "Success or error status, per Pokemon.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Response", "R", "Raw JSON response, per Pokemon.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            var namesOrIds = new List<string>();

            // 2. DA.GetData calls
            DA.GetDataList(0, namesOrIds);

            // 3. Guard clauses / clamping
            var typesTree = new GH_Structure<GH_String>();
            var heightTree = new GH_Structure<GH_Number>();
            var weightTree = new GH_Structure<GH_Number>();
            var baseExpTree = new GH_Structure<GH_Integer>();
            var statNameTree = new GH_Structure<GH_String>();
            var statValueTree = new GH_Structure<GH_Integer>();
            var spriteFrontTree = new GH_Structure<GH_String>();
            var spriteArtworkTree = new GH_Structure<GH_String>();
            var statusTree = new GH_Structure<GH_String>();
            var responseTree = new GH_Structure<GH_String>();

            if (namesOrIds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Pokemon Names Or IDs is empty.");
                DA.SetDataTree(8, statusTree);
                return;
            }

            // 4. Logic — one call per input item, in its own branch
            var client = new PokeDataClient();

            for (int i = 0; i < namesOrIds.Count; i++)
            {
                var path = new GH_Path(i);
                string nameOrId = namesOrIds[i] ?? "";

                if (string.IsNullOrWhiteSpace(nameOrId))
                {
                    statusTree.Append(new GH_String("Error: name/ID is empty."), path);
                    responseTree.Append(new GH_String(""), path);
                    continue;
                }

                var (ok, json, err) = Task.Run(() => client.GetPokemonAsync(nameOrId)).GetAwaiter().GetResult();

                if (!ok)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, nameOrId + ": " + err);
                    statusTree.Append(new GH_String("Error: " + err), path);
                    responseTree.Append(new GH_String(""), path);
                    continue;
                }

                JObject parsed = null;
                try { parsed = JObject.Parse(json); }
                catch (JsonException)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, nameOrId + ": response was not JSON.");
                }

                if (parsed != null)
                {
                    var typesArray = parsed["types"] as JArray;
                    if (typesArray != null)
                        foreach (var t in typesArray)
                            typesTree.Append(new GH_String((string)t["type"]?["name"] ?? ""), path);

                    heightTree.Append(new GH_Number((double?)parsed["height"] ?? 0), path);
                    weightTree.Append(new GH_Number((double?)parsed["weight"] ?? 0), path);
                    baseExpTree.Append(new GH_Integer((int?)parsed["base_experience"] ?? 0), path);

                    var statNames = new List<string>();
                    var statValues = new List<int>();
                    PokeDataParsing.ParseStats(parsed["stats"] as JArray, statNames, statValues);
                    for (int s = 0; s < statNames.Count; s++)
                    {
                        statNameTree.Append(new GH_String(statNames[s]), path);
                        statValueTree.Append(new GH_Integer(statValues[s]), path);
                    }

                    spriteFrontTree.Append(new GH_String((string)parsed["sprites"]?["front_default"] ?? ""), path);
                    spriteArtworkTree.Append(new GH_String((string)parsed["sprites"]?["other"]?["official-artwork"]?["front_default"] ?? ""), path);
                }

                statusTree.Append(new GH_String("OK"), path);
                responseTree.Append(new GH_String(json), path);
            }

            // 5. DA.SetData calls
            DA.SetDataTree(0, typesTree);
            DA.SetDataTree(1, heightTree);
            DA.SetDataTree(2, weightTree);
            DA.SetDataTree(3, baseExpTree);
            DA.SetDataTree(4, statNameTree);
            DA.SetDataTree(5, statValueTree);
            DA.SetDataTree(6, spriteFrontTree);
            DA.SetDataTree(7, spriteArtworkTree);
            DA.SetDataTree(8, statusTree);
            DA.SetDataTree(9, responseTree);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetPokemonBatch;
        public override Guid ComponentGuid => new Guid("0acee750-cca7-47d9-a401-36523e5bfae6");
    }
}

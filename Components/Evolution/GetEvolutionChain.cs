using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Resolves a species name/ID to its evolution chain and flattens it into parent/child edges
    // with trigger conditions. Consolidated so the user does not chain a species lookup into a
    // separate evolution-chain lookup by hand.
    public class GetEvolutionChainComponent : GH_Component
    {
        public GetEvolutionChainComponent()
          : base("Get Evolution Chain", "EvoChain",
              "Resolves a species' evolution chain and flattens it into parent/child edges with trigger conditions.",
              PluginUtilities.TabName, PluginUtilities.CategoryACEvolution)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Species Name Or ID", "SP", "Pokemon species name or numeric ID (e.g. \"bulbasaur\").", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Parent Species", "P", "Parent species name, one entry per evolution edge.", GH_ParamAccess.list);
            pManager.AddTextParameter("Child Species", "C", "Child species name, parallel to Parent Species.", GH_ParamAccess.list);
            pManager.AddTextParameter("Trigger", "TR", "Evolution trigger (e.g. \"level-up\", \"trade\", \"use-item\"), parallel to Parent Species.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response of the evolution chain call.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string speciesNameOrId = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref speciesNameOrId);

            // 3. Guard clauses / clamping
            if (string.IsNullOrWhiteSpace(speciesNameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Species Name Or ID is empty.");
                DA.SetData(3, "Error: Species Name Or ID is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (speciesOk, speciesJson, speciesErr) = Task.Run(() => client.GetPokemonSpeciesAsync(speciesNameOrId)).GetAwaiter().GetResult();

            if (!speciesOk)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, speciesErr);
                DA.SetData(3, "Error: " + speciesErr);
                return;
            }

            string chainUrl;
            try
            {
                var speciesParsed = JObject.Parse(speciesJson);
                chainUrl = PokeDataParsing.SafeString(speciesParsed, "evolution_chain", "url");
            }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Species response was not JSON.");
                DA.SetData(3, "Error: species response was not JSON.");
                return;
            }

            if (string.IsNullOrWhiteSpace(chainUrl))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Species has no evolution_chain URL.");
                DA.SetData(3, "Error: species has no evolution chain.");
                return;
            }

            var (chainOk, chainJson, chainErr) = Task.Run(() => client.GetByAbsoluteUrlAsync(chainUrl)).GetAwaiter().GetResult();

            if (!chainOk)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, chainErr);
                DA.SetData(3, "Error: " + chainErr);
                return;
            }

            var parents = new List<string>();
            var children = new List<string>();
            var triggers = new List<string>();

            try
            {
                var chainParsed = JObject.Parse(chainJson);
                var root = chainParsed["chain"] as JObject;
                if (root != null) PokeDataParsing.FlattenEvolutionChain(root, parents, children, triggers);
            }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Evolution chain response was not JSON.");
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, parents);
            DA.SetDataList(1, children);
            DA.SetDataList(2, triggers);
            DA.SetData(3, "OK");
            DA.SetData(4, chainJson);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_GetEvolutionChain;
        public override Guid ComponentGuid => new Guid("8b7ec50d-7584-4dcb-b647-56e8527f6802");
    }
}

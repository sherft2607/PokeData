using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeData
{
    // Resolves a species' evolution chain (same two-call resolution as Get Evolution Chain) and
    // lays it out as an actual tree: one Point3d per species, one Line per evolution edge, plus
    // parallel edge trigger metadata. Get Evolution Chain gives the flattened edges; this
    // component turns them into placed geometry.
    public class EvolutionTreeComponent : GH_Component
    {
        public EvolutionTreeComponent()
          : base("Evolution Tree", "EvoTree",
              "Lays out a species' evolution chain as tree node points and branch lines.",
              PluginUtilities.TabName, PluginUtilities.CategoryAKGenerative)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Species Name Or ID", "SP", "Pokemon species name or numeric ID (e.g. \"bulbasaur\").", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Horizontal Spacing", "HS", "Spacing between sibling nodes.", GH_ParamAccess.item, 10.0);
            pManager.AddNumberParameter("Vertical Spacing", "VS", "Spacing between generations (depth levels).", GH_ParamAccess.item, 10.0);
            pManager.AddPointParameter("Origin", "O", "Root node placement origin.", GH_ParamAccess.item, Point3d.Origin);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Node Names", "N", "Species name per tree node.", GH_ParamAccess.list);
            pManager.AddPointParameter("Node Points", "P", "Placed point per tree node, parallel to Node Names.", GH_ParamAccess.list);
            pManager.AddLineParameter("Branch Lines", "B", "One line per evolution edge, parent node to child node.", GH_ParamAccess.list);
            pManager.AddTextParameter("Triggers", "TR", "Evolution trigger per branch, parallel to Branch Lines.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw JSON response of the evolution chain call.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string speciesNameOrId = "";
            double horizontalSpacing = 10.0, verticalSpacing = 10.0;
            Point3d origin = Point3d.Origin;

            // 2. DA.GetData calls
            DA.GetData(0, ref speciesNameOrId);
            DA.GetData(1, ref horizontalSpacing);
            DA.GetData(2, ref verticalSpacing);
            DA.GetData(3, ref origin);

            // 3. Guard clauses / clamping
            if (string.IsNullOrWhiteSpace(speciesNameOrId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Species Name Or ID is empty.");
                DA.SetData(4, "Error: Species Name Or ID is empty.");
                return;
            }

            // 4. Logic — same species -> evolution_chain URL -> chain resolution as Get Evolution Chain
            var client = new PokeDataClient();
            var (speciesOk, speciesJson, speciesErr) = Task.Run(() => client.GetPokemonSpeciesAsync(speciesNameOrId)).GetAwaiter().GetResult();

            if (!speciesOk)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, speciesErr);
                DA.SetData(4, "Error: " + speciesErr);
                return;
            }

            string chainUrl, rootSpecies = speciesNameOrId.Trim().ToLowerInvariant();
            try
            {
                var speciesParsed = JObject.Parse(speciesJson);
                chainUrl = PokeDataParsing.SafeString(speciesParsed, "evolution_chain", "url");
            }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Species response was not JSON.");
                DA.SetData(4, "Error: species response was not JSON.");
                return;
            }

            if (string.IsNullOrWhiteSpace(chainUrl))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Species has no evolution_chain URL.");
                DA.SetData(4, "Error: species has no evolution chain.");
                return;
            }

            var (chainOk, chainJson, chainErr) = Task.Run(() => client.GetByAbsoluteUrlAsync(chainUrl)).GetAwaiter().GetResult();

            if (!chainOk)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, chainErr);
                DA.SetData(4, "Error: " + chainErr);
                return;
            }

            var parents = new List<string>();
            var children = new List<string>();
            var triggers = new List<string>();

            try
            {
                var chainParsed = JObject.Parse(chainJson);
                var root = chainParsed["chain"] as JObject;
                if (root != null)
                {
                    rootSpecies = PokeDataParsing.SafeString(root, "species", "name");
                    PokeDataParsing.FlattenEvolutionChain(root, parents, children, triggers);
                }
            }
            catch (JsonException)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Evolution chain response was not JSON.");
            }

            PokeDataParsing.ComputeEvolutionTreeLayout(rootSpecies, parents, children, triggers,
                horizontalSpacing, verticalSpacing,
                out var nodeNames, out var nodeX, out var nodeY,
                out var edgeFrom, out var edgeTo, out var edgeTriggers);

            var nodePoints = new List<Point3d>();
            for (int i = 0; i < nodeNames.Count; i++)
                nodePoints.Add(new Point3d(origin.X + nodeX[i], origin.Y + nodeY[i], origin.Z));

            var branchLines = new List<Line>();
            for (int i = 0; i < edgeFrom.Count; i++)
                branchLines.Add(new Line(nodePoints[edgeFrom[i]], nodePoints[edgeTo[i]]));

            // 5. DA.SetData calls
            DA.SetDataList(0, nodeNames);
            DA.SetDataList(1, nodePoints);
            DA.SetDataList(2, branchLines);
            DA.SetDataList(3, edgeTriggers);
            DA.SetData(4, "OK");
            DA.SetData(5, chainJson);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_EvolutionTree;
        public override Guid ComponentGuid => new Guid("d394a0e3-0894-4c7b-824f-a944295d9f9b");
    }
}

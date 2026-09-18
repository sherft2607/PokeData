using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace PokeData
{
    // Filters two parallel lists (e.g. Pokemon names + a numeric value per Pokemon, such as a
    // stat total) by a named rule against Min/Max, returning the matches plus their original
    // indices. Pure list filtering — not a PokeAPI endpoint; feed it any parallel name/value
    // lists on the canvas (Get Pokemon Batch's names + a computed stat total, height, etc.).
    public class PokemonFilterComponent : GH_Component
    {
        public PokemonFilterComponent()
          : base("Pokemon Filter", "Filter",
              "Filters parallel name/value lists by a rule against Min/Max.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGData)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "N", "Pokemon (or any item) names, parallel to Values.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Values", "V", "Numeric value per item, parallel to Names (e.g. a stat total or height).", GH_ParamAccess.list);
            pManager.AddTextParameter("Filter Rule", "FR", "One of: GreaterThan, LessThan, Between, Equals.", GH_ParamAccess.item, "Between");
            pManager.AddNumberParameter("Min", "MN", "Lower bound (also the equality target for Equals).", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Max", "MX", "Upper bound.", GH_ParamAccess.item, 999999.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Filtered Names", "FN", "Names that matched the filter.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Filtered Values", "FV", "Values that matched the filter, parallel to Filtered Names.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Match Indices", "MI", "Original list index of each match.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            var names = new List<string>();
            var values = new List<double>();
            string rule = "Between";
            double min = 0.0, max = 999999.0;

            // 2. DA.GetData calls
            DA.GetDataList(0, names);
            DA.GetDataList(1, values);
            DA.GetData(2, ref rule);
            DA.GetData(3, ref min);
            DA.GetData(4, ref max);

            // 3. Guard clauses / clamping
            if (names.Count == 0 || values.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Names and Values must both be non-empty.");
                DA.SetData(3, "Error: Names and Values must both be non-empty.");
                return;
            }

            // 4. Logic
            PokeDataParsing.FilterByRule(names, values, rule, min, max,
                out var filteredNames, out var filteredValues, out var matchIndices);

            // 5. DA.SetData calls
            DA.SetDataList(0, filteredNames);
            DA.SetDataList(1, filteredValues);
            DA.SetDataList(2, matchIndices);
            DA.SetData(3, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_PokemonFilter;
        public override Guid ComponentGuid => new Guid("1c4481b8-aa75-4678-9a80-14d691b90bcc");
    }
}

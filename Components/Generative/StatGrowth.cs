using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace PokeData
{
    // Computes exact runtime battle stats from base stats, level, IVs, EVs, and a nature's
    // increased/decreased stat (e.g. Get Nature's outputs) — the standard Generation III+
    // formulas. Pure math — not a PokeAPI endpoint.
    public class StatGrowthComponent : GH_Component
    {
        private static readonly string[] StatOrder = { "hp", "attack", "defense", "special-attack", "special-defense", "speed" };

        public StatGrowthComponent()
          : base("Stat Growth", "Growth",
              "Computes exact runtime battle stats from base stats, level, IVs, EVs, and nature.",
              PluginUtilities.TabName, PluginUtilities.CategoryAKGenerative)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Base Stats", "BS", "Base stat values, in order: hp, attack, defense, special-attack, special-defense, speed.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Level", "L", "Pokemon level, 1-100.", GH_ParamAccess.item, 50);
            pManager.AddIntegerParameter("IVs", "IV", "Individual Values, 0-31 each, parallel to Base Stats. Defaults to 31 (perfect) for any missing entry.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("EVs", "EV", "Effort Values, 0-252 each, parallel to Base Stats. Defaults to 0 for any missing entry.", GH_ParamAccess.list);
            pManager.AddTextParameter("Increased Stat", "IS", "Nature's increased stat name, e.g. Get Nature's Increased Stat output.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Decreased Stat", "DS", "Nature's decreased stat name, e.g. Get Nature's Decreased Stat output.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Computed Stats", "CS", "Runtime battle stats, parallel to Base Stats.", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            var baseStats = new List<int>();
            int level = 50;
            var ivs = new List<int>();
            var evs = new List<int>();
            string increasedStat = "", decreasedStat = "";

            // 2. DA.GetData calls
            DA.GetDataList(0, baseStats);
            DA.GetData(1, ref level);
            DA.GetDataList(2, ivs);
            DA.GetDataList(3, evs);
            DA.GetData(4, ref increasedStat);
            DA.GetData(5, ref decreasedStat);

            // 3. Guard clauses / clamping
            if (baseStats.Count != 6)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Base Stats needs exactly 6 values (hp, attack, defense, special-attack, special-defense, speed).");
                DA.SetData(1, "Error: Base Stats needs exactly 6 values.");
                return;
            }

            level = Math.Max(1, Math.Min(100, level));

            // 4. Logic
            var computed = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                int iv = i < ivs.Count ? Math.Max(0, Math.Min(31, ivs[i])) : 31;
                int ev = i < evs.Count ? Math.Max(0, Math.Min(252, evs[i])) : 0;

                if (i == 0)
                {
                    computed.Add(PokeDataParsing.ComputeHpStat(baseStats[i], iv, ev, level));
                }
                else
                {
                    double natureMultiplier = PokeDataParsing.NatureMultiplierFor(increasedStat, decreasedStat, StatOrder[i]);
                    computed.Add(PokeDataParsing.ComputeBattleStat(baseStats[i], iv, ev, level, natureMultiplier));
                }
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, computed);
            DA.SetData(1, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_StatGrowth;
        public override Guid ComponentGuid => new Guid("32e65bec-328a-435d-a58d-4bb25965011c");
    }
}

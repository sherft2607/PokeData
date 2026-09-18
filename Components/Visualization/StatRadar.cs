using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace PokeData
{
    // Turns a Pokemon's 6 base stat values (e.g. Get Pokemon's Stat Values output) into a
    // normalized 2D radar/spider chart polygon for parametric stat-comparison geometry. Pure
    // canvas geometry — not a PokeAPI endpoint.
    public class StatRadarComponent : GH_Component
    {
        public StatRadarComponent()
          : base("Stat Radar", "Radar",
              "Turns base stat values into a normalized 2D radar chart polyline.",
              PluginUtilities.TabName, PluginUtilities.CategoryAIVisualization)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Stat Values", "SV", "Base stat values, e.g. Get Pokemon's Stat Values output.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "R", "Radius at maximum stat value.", GH_ParamAccess.item, 10.0);
            pManager.AddNumberParameter("Max Stat", "MX", "Stat value that maps to the full radius (canonical base-stat ceiling is 255).", GH_ParamAccess.item, 255.0);
            pManager.AddPointParameter("Center", "C", "Center point of the radar chart.", GH_ParamAccess.item, Point3d.Origin);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Radar Points", "RP", "One point per stat value, evenly spaced around Center.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Radar Polyline", "RPL", "Closed polyline through the radar points.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            var statValues = new List<int>();
            double radius = 10.0, maxStat = 255.0;
            Point3d center = Point3d.Origin;

            // 2. DA.GetData calls
            DA.GetDataList(0, statValues);
            DA.GetData(1, ref radius);
            DA.GetData(2, ref maxStat);
            DA.GetData(3, ref center);

            // 3. Guard clauses / clamping
            if (statValues.Count < 3)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Stat Values needs at least 3 values to form a radar polygon.");
                DA.SetData(2, "Error: Stat Values needs at least 3 values.");
                return;
            }

            // 4. Logic
            PokeDataParsing.ComputeStatRadarPoints(statValues, radius, maxStat, out var xs, out var ys);

            var points = new List<Point3d>();
            for (int i = 0; i < xs.Count; i++)
                points.Add(new Point3d(center.X + xs[i], center.Y + ys[i], center.Z));

            var polyline = new Polyline(points);
            polyline.Add(points[0]);

            // 5. DA.SetData calls
            DA.SetDataList(0, points);
            DA.SetData(1, polyline.ToPolylineCurve());
            DA.SetData(2, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_StatRadar;
        public override Guid ComponentGuid => new Guid("3d2e3b8f-30aa-4fe0-bd3e-f23e06c08d02");
    }
}

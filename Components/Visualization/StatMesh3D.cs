using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokeData
{
    // Extrudes a fixed-radius regular polygon into a 3D "radar skyline" Mesh, where each vertex's
    // height is driven by one base stat value — a 3D counterpart to Stat Radar's flat polyline.
    // Pure canvas geometry — not a PokeAPI endpoint.
    public class StatMesh3DComponent : GH_Component
    {
        public StatMesh3DComponent()
          : base("Stat Mesh 3D", "StatMesh",
              "Extrudes base stat values into a 3D radar Mesh, one vertex height per stat.",
              PluginUtilities.TabName, PluginUtilities.CategoryAIVisualization)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Stat Values", "SV", "Base stat values, e.g. Get Pokemon's Stat Values output.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height Factor", "HF", "Scales how tall the extrusion gets at the maximum stat value.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Radius", "R", "Radius of the base/top ring.", GH_ParamAccess.item, 10.0);
            pManager.AddNumberParameter("Max Stat", "MX", "Stat value that maps to full extrusion height (canonical base-stat ceiling is 255).", GH_ParamAccess.item, 255.0);
            pManager.AddPointParameter("Center", "C", "Center point of the mesh.", GH_ParamAccess.item, Point3d.Origin);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The extruded 3D radar mesh.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            var statValues = new List<int>();
            double heightFactor = 1.0, radius = 10.0, maxStat = 255.0;
            Point3d center = Point3d.Origin;

            // 2. DA.GetData calls
            DA.GetDataList(0, statValues);
            DA.GetData(1, ref heightFactor);
            DA.GetData(2, ref radius);
            DA.GetData(3, ref maxStat);
            DA.GetData(4, ref center);

            // 3. Guard clauses / clamping
            if (statValues.Count < 3)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Stat Values needs at least 3 values to form a mesh.");
                DA.SetData(1, "Error: Stat Values needs at least 3 values.");
                return;
            }

            // 4. Logic
            int n = statValues.Count;
            PokeDataParsing.ComputeRegularPolygonPoints(n, radius, out var ringX, out var ringY);
            var heights = PokeDataParsing.ComputeStatMeshHeights(statValues, maxStat, heightFactor * radius);

            var mesh = new Mesh();
            for (int i = 0; i < n; i++)
                mesh.Vertices.Add(center.X + ringX[i], center.Y + ringY[i], center.Z);
            for (int i = 0; i < n; i++)
                mesh.Vertices.Add(center.X + ringX[i], center.Y + ringY[i], center.Z + heights[i]);

            int bottomCenter = mesh.Vertices.Add(center);
            double avgTopZ = heights.Count > 0 ? heights.Average() : 0.0;
            int topCenter = mesh.Vertices.Add(new Point3d(center.X, center.Y, center.Z + avgTopZ));

            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                int b0 = i, b1 = next, t0 = n + i, t1 = n + next;

                // side wall quad
                mesh.Faces.AddFace(b0, b1, t1, t0);
                // bottom cap triangle
                mesh.Faces.AddFace(bottomCenter, b1, b0);
                // top cap triangle
                mesh.Faces.AddFace(topCenter, t0, t1);
            }

            mesh.Normals.ComputeNormals();
            mesh.Compact();

            // 5. DA.SetData calls
            DA.SetData(0, mesh);
            DA.SetData(1, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_StatMesh3D;
        public override Guid ComponentGuid => new Guid("40a65482-255d-4e17-bd56-b5d0e8ffb412");
    }
}

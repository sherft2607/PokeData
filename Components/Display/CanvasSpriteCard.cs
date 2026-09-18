using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace PokeData
{
    // Builds a flat rectangular card Mesh at a point, sized to hold a sprite + stat text, plus a
    // combined label string and a display Plane for native GH components (Picture Frame, Text
    // Tag 3D) to consume — no texture is baked onto the mesh here, since that needs a display
    // conduit rather than mesh geometry. Pure canvas layout — not a PokeAPI endpoint.
    public class CanvasSpriteCardComponent : GH_Component
    {
        public CanvasSpriteCardComponent()
          : base("Canvas Sprite Card", "Card",
              "Builds a flat card Mesh + label + plane for laying out a sprite and stats in the viewport.",
              PluginUtilities.TabName, PluginUtilities.CategoryAJDisplay)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Pokemon (or card title) name.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Stats", "ST", "Stat display lines, e.g. \"hp: 35\" per item.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Sprite Bitmap", "B", "Decoded sprite Bitmap (e.g. Sprite Downloader's output) — passed through for a Picture Frame.", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "Card placement point (lower-left corner).", GH_ParamAccess.item, Point3d.Origin);
            pManager.AddNumberParameter("Width", "W", "Card width.", GH_ParamAccess.item, 10.0);
            pManager.AddNumberParameter("Height", "H", "Card height.", GH_ParamAccess.item, 14.0);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Card Mesh", "CM", "Flat rectangular card backing.", GH_ParamAccess.item);
            pManager.AddTextParameter("Label", "L", "Name and stat lines joined into one label string.", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Card Plane", "CP", "Plane at Point, for wiring into Picture Frame / Text Tag 3D.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Sprite Bitmap", "B", "Passthrough of the input Bitmap, for convenience.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string name = "";
            var stats = new List<string>();
            object bitmap = null;
            Point3d point = Point3d.Origin;
            double width = 10.0, height = 14.0;

            // 2. DA.GetData calls
            DA.GetData(0, ref name);
            DA.GetDataList(1, stats);
            DA.GetData(2, ref bitmap);
            DA.GetData(3, ref point);
            DA.GetData(4, ref width);
            DA.GetData(5, ref height);

            // 3. Guard clauses / clamping
            if (width <= 0) width = 10.0;
            if (height <= 0) height = 14.0;

            // 4. Logic
            var plane = new Plane(point, Vector3d.ZAxis);

            var mesh = new Mesh();
            mesh.Vertices.Add(point.X, point.Y, point.Z);
            mesh.Vertices.Add(point.X + width, point.Y, point.Z);
            mesh.Vertices.Add(point.X + width, point.Y + height, point.Z);
            mesh.Vertices.Add(point.X, point.Y + height, point.Z);
            mesh.Faces.AddFace(0, 1, 2, 3);
            mesh.Normals.ComputeNormals();

            string label = PokeDataParsing.BuildCardLabel(name, stats);

            // 5. DA.SetData calls
            DA.SetData(0, mesh);
            DA.SetData(1, label);
            DA.SetData(2, plane);
            DA.SetData(3, bitmap);
            DA.SetData(4, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_CanvasSpriteCard;
        public override Guid ComponentGuid => new Guid("5cbbe50e-fc10-45d2-81df-1709a8057268");
    }
}

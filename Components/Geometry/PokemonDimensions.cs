using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace PokeData
{
    // Converts a Pokemon's raw PokeAPI height/weight (decimetres/hectograms — e.g. Get Pokemon's
    // Height/Weight outputs) into metric and imperial units, plus a reference bounding Box sized
    // to the converted height. Pure conversion — not a PokeAPI endpoint.
    public class PokemonDimensionsComponent : GH_Component
    {
        public PokemonDimensionsComponent()
          : base("Pokemon Dimensions", "Dims",
              "Converts PokeAPI height/weight into metric and imperial units, plus a reference Box.",
              PluginUtilities.TabName, PluginUtilities.CategoryAHGeometry)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Height", "H", "Height in decimetres, e.g. Get Pokemon's Height output.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Weight", "W", "Weight in hectograms, e.g. Get Pokemon's Weight output.", GH_ParamAccess.item, 0.0);
            pManager.AddPlaneParameter("Plane", "PL", "Base plane for the reference Box.", GH_ParamAccess.item, Plane.WorldXY);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Height (m)", "HM", "Height in meters.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height (ft)", "HF", "Height in feet.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight (kg)", "WK", "Weight in kilograms.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight (lb)", "WL", "Weight in pounds.", GH_ParamAccess.item);
            pManager.AddBoxParameter("Reference Box", "BX", "A box sized to the converted height, footprint proportional to it.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            double heightDm = 0.0, weightHg = 0.0;
            Plane plane = Plane.WorldXY;

            // 2. DA.GetData calls
            DA.GetData(0, ref heightDm);
            DA.GetData(1, ref weightHg);
            DA.GetData(2, ref plane);

            // 3. Guard clauses / clamping
            if (heightDm < 0) heightDm = 0;
            if (weightHg < 0) weightHg = 0;

            // 4. Logic
            PokeDataParsing.ConvertDimensions(heightDm, weightHg, out var heightM, out var heightFt, out var weightKg, out var weightLb);

            double footprint = Math.Max(heightM * 0.4, 0.01);
            var box = new Box(plane,
                new Interval(-footprint / 2, footprint / 2),
                new Interval(-footprint / 2, footprint / 2),
                new Interval(0, Math.Max(heightM, 0.01)));

            // 5. DA.SetData calls
            DA.SetData(0, heightM);
            DA.SetData(1, heightFt);
            DA.SetData(2, weightKg);
            DA.SetData(3, weightLb);
            DA.SetData(4, box);
            DA.SetData(5, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_PokemonDimensions;
        public override Guid ComponentGuid => new Guid("6446f529-9b85-4191-8235-267523e36a85");
    }
}

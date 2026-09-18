using Grasshopper.Kernel;
using System;

namespace PokeData
{
    // Returns the canonical display colors for one or two elemental types, plus a blended color
    // for dual-type swatches. Pure lookup — not a PokeAPI endpoint.
    public class TypePaletteComponent : GH_Component
    {
        public TypePaletteComponent()
          : base("Type Palette", "Palette",
              "Canonical display colors for one or two elemental types, plus a blended dual-type color.",
              PluginUtilities.TabName, PluginUtilities.CategoryAJDisplay)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Type 1", "T1", "First (or only) elemental type name.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Type 2", "T2", "Second elemental type name — leave empty for a single-type Pokemon.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddColourParameter("Color 1", "C1", "Canonical color for Type 1.", GH_ParamAccess.item);
            pManager.AddColourParameter("Color 2", "C2", "Canonical color for Type 2 (same as Color 1 if Type 2 is empty).", GH_ParamAccess.item);
            pManager.AddColourParameter("Blend Color", "CB", "Average-channel blend of Color 1 and Color 2.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string type1 = "", type2 = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref type1);
            DA.GetData(1, ref type2);

            // 3. Guard clauses / clamping
            if (string.IsNullOrWhiteSpace(type1))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Type 1 is empty.");
                DA.SetData(3, "Error: Type 1 is empty.");
                return;
            }

            // 4. Logic
            var color1 = PluginUtilities.TypeColor(type1);
            var color2 = string.IsNullOrWhiteSpace(type2) ? color1 : PluginUtilities.TypeColor(type2);
            var blend = PluginUtilities.BlendColors(color1, color2);

            // 5. DA.SetData calls
            DA.SetData(0, color1);
            DA.SetData(1, color2);
            DA.SetData(2, blend);
            DA.SetData(3, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_TypePalette;
        public override Guid ComponentGuid => new Guid("6bdf77e0-c4de-40b2-961f-a0f08bae9481");
    }
}

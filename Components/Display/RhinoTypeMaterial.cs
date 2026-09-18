using Grasshopper.Kernel;
using System;

namespace PokeData
{
    // Builds a simple diffuse Rhino.Render.RenderMaterial from one or two canonical type colors
    // (Type Palette's same lookup/blend), optionally adding it to the active document's render
    // material table. Pure lookup plus a native Rhino material wrapper — not a PokeAPI endpoint.
    public class RhinoTypeMaterialComponent : GH_Component
    {
        public RhinoTypeMaterialComponent()
          : base("Rhino Type Material", "TypeMat",
              "Builds a simple diffuse RenderMaterial from one or two canonical type colors.",
              PluginUtilities.TabName, PluginUtilities.CategoryAJDisplay)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Type 1", "T1", "First (or only) elemental type name.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Type 2", "T2", "Second elemental type name — leave empty for a single-type Pokemon.", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Add To Document", "AD", "Set true to add the material to the active Rhino document's render material table.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "The built Rhino.Render.RenderMaterial.", GH_ParamAccess.item);
            pManager.AddColourParameter("Color", "C", "The diffuse color used (Color 1 alone, or the blend of Color 1/Color 2).", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string type1 = "", type2 = "";
            bool addToDocument = false;

            // 2. DA.GetData calls
            DA.GetData(0, ref type1);
            DA.GetData(1, ref type2);
            DA.GetData(2, ref addToDocument);

            // 3. Guard clauses / clamping
            if (string.IsNullOrWhiteSpace(type1))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Type 1 is empty.");
                DA.SetData(2, "Error: Type 1 is empty.");
                return;
            }

            // 4. Logic
            var color1 = PluginUtilities.TypeColor(type1);
            bool hasType2 = !string.IsNullOrWhiteSpace(type2);
            var color2 = hasType2 ? PluginUtilities.TypeColor(type2) : color1;
            var diffuseColor = hasType2 ? PluginUtilities.BlendColors(color1, color2) : color1;

            var docMaterial = new Rhino.DocObjects.Material
            {
                Name = "PokeData " + type1.Trim().ToLowerInvariant() + (hasType2 ? "-" + type2.Trim().ToLowerInvariant() : ""),
                DiffuseColor = diffuseColor
            };

            Rhino.Render.RenderMaterial renderMaterial = null;
            string status = "OK";
            try
            {
                renderMaterial = docMaterial.RenderMaterial;

                if (addToDocument)
                {
                    var doc = Rhino.RhinoDoc.ActiveDoc;
                    if (doc == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No active Rhino document — material was built but not added.");
                        status = "OK (not added — no active document)";
                    }
                    else
                    {
                        doc.RenderMaterials.Add(renderMaterial);
                    }
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not build/add render material: " + ex.Message);
                DA.SetData(2, "Error: " + ex.Message);
                return;
            }

            // 5. DA.SetData calls
            DA.SetData(0, renderMaterial);
            DA.SetData(1, diffuseColor);
            DA.SetData(2, status);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_RhinoTypeMaterial;
        public override Guid ComponentGuid => new Guid("e1d1df24-8721-4a8b-9406-cbb04f353842");
    }
}

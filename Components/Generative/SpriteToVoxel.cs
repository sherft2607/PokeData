using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Drawing;

namespace PokeData
{
    // Voxelizes an in-memory sprite Bitmap (e.g. Sprite Downloader's output) into a box mesh —
    // one box per opaque pixel, with per-vertex RGBA vertex colors — plus a separate luminance
    // heightfield relief mesh. Pure canvas geometry — not a PokeAPI endpoint.
    public class SpriteToVoxelComponent : GH_Component
    {
        public SpriteToVoxelComponent()
          : base("Sprite To Voxel", "Voxel",
              "Turns a sprite Bitmap into a voxel box mesh (RGBA vertex colors) and a luminance heightfield mesh.",
              PluginUtilities.TabName, PluginUtilities.CategoryAKGenerative)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // AddGenericParameter registers a Param_GenericObject — the same param type Sprite
            // Downloader's output and Canvas Sprite Card's input use, so nothing rejects the
            // wrapper upstream; this is not a typed param that would refuse a non-Bitmap wrapper.
            pManager.AddGenericParameter("Sprite Bitmap", "B", "Decoded sprite Bitmap, e.g. Sprite Downloader's output.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Voxel Size", "VS", "Edge length of each voxel box.", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Alpha Threshold", "AT", "Pixels with alpha at or below this are treated as transparent/background, 0-255.", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Heightfield Scale", "HS", "Scales the luminance heightfield's Z displacement.", GH_ParamAccess.item, 5.0);
            pManager.AddIntegerParameter("Max Resolution", "MR", "Downsamples so neither dimension exceeds this many pixels — official artwork can be 475x475+, which would otherwise produce an unusably large mesh.", GH_ParamAccess.item, 64);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Voxel Mesh", "VM", "One box per opaque pixel, with per-vertex RGBA vertex colors.", GH_ParamAccess.item);
            pManager.AddMeshParameter("Heightfield Mesh", "HM", "A grid mesh displaced by per-pixel luminance.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Voxel Count", "VC", "Number of opaque pixels voxelized.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            double voxelSize = 1.0, heightfieldScale = 5.0;
            int alphaThreshold = 10, maxResolution = 64;

            // 2. DA.GetData calls
            // Try the direct-typed overload first — DA.GetData<T> resolves T from the `ref`
            // argument, and GH_ObjectWrapper.CastTo<T> (called internally for a generic-param
            // source) unwraps to the boxed value when T matches its runtime type. This is the
            // idiomatic path and should already succeed for a Bitmap wired straight from Sprite
            // Downloader's generic output.
            Bitmap sourceBitmap = null;
            bool gotTyped = DA.GetData(0, ref sourceBitmap);

            // Fallback: inspect the raw object Grasshopper actually delivered. Also serves as a
            // live diagnostic — the runtime type is reported via a Remark message so it shows up
            // on the component without treating it as an error.
            object rawInput = null;
            DA.GetData(0, ref rawInput);
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                "Sprite Bitmap raw input type: " + (rawInput == null ? "null" : rawInput.GetType().FullName)
                + " | typed GetData<Bitmap> succeeded: " + gotTyped);

            if (sourceBitmap == null)
            {
                if (rawInput is Grasshopper.Kernel.Types.GH_ObjectWrapper wrapper)
                    sourceBitmap = wrapper.Value as Bitmap;
                else if (rawInput is Bitmap bmp)
                    sourceBitmap = bmp;
            }

            DA.GetData(1, ref voxelSize);
            DA.GetData(2, ref alphaThreshold);
            DA.GetData(3, ref heightfieldScale);
            DA.GetData(4, ref maxResolution);

            // 3. Guard clauses / clamping
            if (sourceBitmap == null)
            {
                string typeInfo = rawInput == null ? "null" : rawInput.GetType().FullName;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Sprite Bitmap is empty or not a Bitmap (raw input type: " + typeInfo + ").");
                DA.SetData(3, "Error: Sprite Bitmap is empty or not a Bitmap (raw type: " + typeInfo + ").");
                return;
            }

            if (voxelSize <= 0) voxelSize = 1.0;
            alphaThreshold = Math.Max(0, Math.Min(255, alphaThreshold));
            maxResolution = Math.Max(4, maxResolution);

            // 4. Logic — downsample first so official artwork (475x475+) doesn't produce an
            // unusably large per-pixel mesh
            Bitmap bitmap = sourceBitmap;
            if (sourceBitmap.Width > maxResolution || sourceBitmap.Height > maxResolution)
            {
                double scale = Math.Min((double)maxResolution / sourceBitmap.Width, (double)maxResolution / sourceBitmap.Height);
                int newWidth = Math.Max(1, (int)(sourceBitmap.Width * scale));
                int newHeight = Math.Max(1, (int)(sourceBitmap.Height * scale));
                bitmap = new Bitmap(sourceBitmap, newWidth, newHeight);
            }

            int width = bitmap.Width, height = bitmap.Height;

            var voxelMesh = new Mesh();
            int voxelCount = 0;

            var heightfield = new Mesh();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    double lum = PokeDataParsing.Luminance(pixel.R, pixel.G, pixel.B);
                    double z = pixel.A > 0 ? lum * heightfieldScale : 0.0;
                    heightfield.Vertices.Add(x * voxelSize, -y * voxelSize, z);
                    heightfield.VertexColors.Add(pixel);
                }
            for (int y = 0; y < height - 1; y++)
                for (int x = 0; x < width - 1; x++)
                {
                    int i00 = y * width + x, i10 = y * width + (x + 1);
                    int i01 = (y + 1) * width + x, i11 = (y + 1) * width + (x + 1);
                    heightfield.Faces.AddFace(i00, i10, i11, i01);
                }
            heightfield.Normals.ComputeNormals();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if (!PokeDataParsing.IsOpaquePixel(pixel.A, alphaThreshold)) continue;

                    voxelCount++;
                    double x0 = x * voxelSize, x1 = (x + 1) * voxelSize;
                    double y0 = -y * voxelSize, y1 = -(y + 1) * voxelSize;
                    double z0 = 0.0, z1 = voxelSize;

                    int baseIndex = voxelMesh.Vertices.Count;
                    voxelMesh.Vertices.Add(x0, y0, z0);
                    voxelMesh.Vertices.Add(x1, y0, z0);
                    voxelMesh.Vertices.Add(x1, y1, z0);
                    voxelMesh.Vertices.Add(x0, y1, z0);
                    voxelMesh.Vertices.Add(x0, y0, z1);
                    voxelMesh.Vertices.Add(x1, y0, z1);
                    voxelMesh.Vertices.Add(x1, y1, z1);
                    voxelMesh.Vertices.Add(x0, y1, z1);

                    for (int v = 0; v < 8; v++) voxelMesh.VertexColors.Add(pixel);

                    voxelMesh.Faces.AddFace(baseIndex + 0, baseIndex + 1, baseIndex + 2, baseIndex + 3); // bottom
                    voxelMesh.Faces.AddFace(baseIndex + 4, baseIndex + 7, baseIndex + 6, baseIndex + 5); // top
                    voxelMesh.Faces.AddFace(baseIndex + 0, baseIndex + 4, baseIndex + 5, baseIndex + 1); // front
                    voxelMesh.Faces.AddFace(baseIndex + 1, baseIndex + 5, baseIndex + 6, baseIndex + 2); // right
                    voxelMesh.Faces.AddFace(baseIndex + 2, baseIndex + 6, baseIndex + 7, baseIndex + 3); // back
                    voxelMesh.Faces.AddFace(baseIndex + 3, baseIndex + 7, baseIndex + 4, baseIndex + 0); // left
                }
            }
            voxelMesh.Normals.ComputeNormals();

            // 5. DA.SetData calls
            DA.SetData(0, voxelMesh);
            DA.SetData(1, heightfield);
            DA.SetData(2, voxelCount);
            DA.SetData(3, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_SpriteToVoxel;
        public override Guid ComponentGuid => new Guid("2ff2322c-01f9-4ecd-a766-ad257e2d4171");
    }
}

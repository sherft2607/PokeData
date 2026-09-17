using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace PokeData
{
    // Downloads an arbitrary image URL (e.g. the Sprite Front/Artwork URL outputs from Get
    // Pokemon) and decodes it to a Bitmap on the canvas for use with image samplers. Not a
    // PokeAPI endpoint — sprites are hosted on raw.githubusercontent.com.
    public class SpriteDownloaderComponent : GH_Component
    {
        public SpriteDownloaderComponent()
          : base("Sprite Downloader", "SpriteDL",
              "Downloads an image URL and decodes it to a Bitmap for use with image samplers.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAPokemon)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Sprite URL", "SU", "Image URL, e.g. Get Pokemon's Sprite Front URL or Sprite Artwork URL output.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap", "B", "Decoded image, as a System.Drawing.Bitmap.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Success or error status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            string spriteUrl = "";

            // 2. DA.GetData calls
            DA.GetData(0, ref spriteUrl);

            // 3. Guard clauses / clamping
            if (string.IsNullOrWhiteSpace(spriteUrl))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Sprite URL is empty.");
                DA.SetData(1, "Error: Sprite URL is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var (ok, bytes, err) = Task.Run(() => client.GetImageBytesAsync(spriteUrl)).GetAwaiter().GetResult();

            if (!ok)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(1, "Error: " + err);
                return;
            }

            Bitmap bitmap = null;
            try
            {
                using (var stream = new MemoryStream(bytes))
                    bitmap = new Bitmap(stream);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not decode image: " + ex.Message);
                DA.SetData(1, "Error: could not decode image.");
                return;
            }

            // 5. DA.SetData calls
            DA.SetData(0, bitmap);
            DA.SetData(1, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_SpriteDownloader;
        public override Guid ComponentGuid => new Guid("438ab34d-8221-427a-8271-1dc251e3f2cd");
    }
}

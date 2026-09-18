using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PokeData
{
    // Downloads a list of image URLs in parallel (Task.WhenAll over the same in-memory
    // GetImageBytesAsync helper Sprite Downloader uses) and decodes each to a Bitmap. Gated by
    // Trigger so a list of URLs on the canvas doesn't refetch on every solve. Not a PokeAPI
    // endpoint — sprites/artwork are hosted on raw.githubusercontent.com.
    public class BatchDownloaderComponent : GH_Component
    {
        public BatchDownloaderComponent()
          : base("Batch Downloader", "BatchDL",
              "Downloads a list of image URLs in parallel and decodes each to a Bitmap, on trigger.",
              PluginUtilities.TabName, PluginUtilities.CategoryAAPokemon)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("URLs", "U", "Image URLs to download, e.g. a list of sprite URLs.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Trigger", "T", "Set true to download every URL.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmaps", "B", "Decoded Bitmaps, parallel to URLs (null entry where a download or decode failed).", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "S", "Overall success/failure summary.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare variables with defaults
            var urls = new List<string>();
            bool trigger = false;

            // 2. DA.GetData calls
            DA.GetDataList(0, urls);
            DA.GetData(1, ref trigger);

            // 3. Guard clauses / clamping
            if (!trigger)
            {
                DA.SetDataList(0, new List<Bitmap>());
                DA.SetData(1, "Waiting for trigger.");
                return;
            }

            if (urls.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "URLs is empty.");
                DA.SetData(1, "Error: URLs is empty.");
                return;
            }

            // 4. Logic
            var client = new PokeDataClient();
            var downloadTasks = urls.Select(url => client.GetImageBytesAsync(url)).ToArray();
            var results = Task.Run(() => Task.WhenAll(downloadTasks)).GetAwaiter().GetResult();

            var bitmaps = new List<Bitmap>();
            int successCount = 0, failureCount = 0;

            foreach (var (ok, bytes, err) in results)
            {
                if (!ok)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Download failed: " + err);
                    bitmaps.Add(null);
                    failureCount++;
                    continue;
                }

                try
                {
                    using (var stream = new MemoryStream(bytes))
                        bitmaps.Add(new Bitmap(stream));
                    successCount++;
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not decode image: " + ex.Message);
                    bitmaps.Add(null);
                    failureCount++;
                }
            }

            // 5. DA.SetData calls
            DA.SetDataList(0, bitmaps);
            DA.SetData(1, PokeDataParsing.SummarizeBatchStatus(successCount, failureCount));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.PK_BatchDownloader;
        public override Guid ComponentGuid => new Guid("2ecb5a7b-a4b8-44c5-b175-b9e675320696");
    }
}

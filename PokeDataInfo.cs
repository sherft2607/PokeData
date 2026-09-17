using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace PokeData
{
    public class PokeDataInfo : GH_AssemblyInfo
    {
        public override string Name => "PokeData";
        // Same 16x16 logo as the category icon — shown in Grasshopper's loaded-libraries list
        public override Bitmap Icon => Properties.Resources.PK_PokeDataLogo;
        public override string Description => "Look up Pokemon, types, evolution chains, moves and abilities from PokeAPI directly on the Grasshopper canvas.";
        public override Guid Id => new Guid("e8e46da3-bc4a-4fc3-9b2d-3a9c37971ff6");
        public override string AuthorName => "Shandon Herft";
        public override string AuthorContact => "shandonherft@gmail.com";
        // Single source of truth is the .csproj <Version> — never a second hardcoded string here
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString(3);
    }

    public class PokeDataCategoryIcon : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("PokeData", Properties.Resources.PK_PokeDataLogo);
            Instances.ComponentServer.AddCategorySymbolName("PokeData", 'P');
            return GH_LoadingInstruction.Proceed;
        }
    }
}

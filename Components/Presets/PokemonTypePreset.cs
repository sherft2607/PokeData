using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PokeData.Presets
{
    // Fixed dropdown of the 18 elemental types, for wiring into Get Type / Get Type Matrix
    // instead of typing type names by hand.
    public class PokemonTypePresetComponent : GH_ValueList
    {
        public PokemonTypePresetComponent()
        {
            Category = PluginUtilities.TabName;
            SubCategory = PluginUtilities.CategoryZYPresets;
            Name = "Type Name";
            NickName = "TN";
            Description = "Fixed list of the 18 elemental Pokemon types.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Normal",   "normal" },
            { "Fire",     "fire" },
            { "Water",    "water" },
            { "Electric", "electric" },
            { "Grass",    "grass" },
            { "Ice",      "ice" },
            { "Fighting", "fighting" },
            { "Poison",   "poison" },
            { "Ground",   "ground" },
            { "Flying",   "flying" },
            { "Psychic",  "psychic" },
            { "Bug",      "bug" },
            { "Rock",     "rock" },
            { "Ghost",    "ghost" },
            { "Dragon",   "dragon" },
            { "Dark",     "dark" },
            { "Steel",    "steel" },
            { "Fairy",    "fairy" },
        };

        protected override Bitmap Icon => Properties.Resources.PK_PokemonTypePreset;
        public override Guid ComponentGuid => new Guid("c6cd2f82-c5e8-41f9-a284-3ca1895d5c9c");
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace PokeData
{
    public static class PluginUtilities
    {
        // Shared enum helper — used by any preset component backed by an enum
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        internal static readonly string TabName = "PokeData";

        // Subcategory constants — one per subcategory, in display order.
        // No Auth subcategory: PokeAPI is fully public and unauthenticated.
        internal static readonly string CategoryAAPokemon    = "1. Pokemon";
        internal static readonly string CategoryABTypes      = "2. Types";
        internal static readonly string CategoryACEvolution  = "3. Evolution";
        internal static readonly string CategoryADMoves      = "4. Moves";
        internal static readonly string CategoryZYPresets    = "5. Presets";
        internal static readonly string CategoryZZUtilities  = "6. Utilities";
    }
}

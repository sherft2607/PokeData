using System;
using System.Collections.Generic;
using System.Drawing;
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
        internal static readonly string CategoryAAPokemon       = "1. Pokemon";
        internal static readonly string CategoryABTypes         = "2. Types";
        internal static readonly string CategoryACEvolution     = "3. Evolution";
        internal static readonly string CategoryADMoves         = "4. Moves";
        internal static readonly string CategoryAEItems         = "5. Items";
        internal static readonly string CategoryAFStats         = "6. Stats";
        internal static readonly string CategoryAGData          = "7. Data";
        internal static readonly string CategoryAHGeometry      = "8. Geometry";
        internal static readonly string CategoryAIVisualization = "9. Visualization";
        internal static readonly string CategoryAJDisplay       = "10. Display";
        internal static readonly string CategoryZYPresets       = "11. Presets";
        internal static readonly string CategoryZZUtilities     = "12. Utilities";

        // v2.1.0: the 18 canonical elemental types, in the same fixed order used throughout the
        // plugin (matches GetTypeMatrixComponent's own list) — the "attacking type" universe for
        // Dual Type Matchup's per-type bucketing.
        internal static readonly string[] AllTypeNames = new[]
        {
            "normal", "fire", "water", "electric", "grass", "ice", "fighting", "poison",
            "ground", "flying", "psychic", "bug", "rock", "ghost", "dragon", "dark", "steel", "fairy"
        };

        // v2.0.0: canonical elemental-type colors (the community-standard palette used by
        // Pokemon type charts/badges) for viewport display pipelines. Unknown type names
        // fall back to a neutral gray rather than throwing.
        private static readonly Dictionary<string, Color> _typeColors = new Dictionary<string, Color>
        {
            { "normal",   Color.FromArgb(168, 168, 120) },
            { "fire",     Color.FromArgb(240, 128,  48) },
            { "water",    Color.FromArgb( 104, 144, 240) },
            { "electric", Color.FromArgb(248, 208,  48) },
            { "grass",    Color.FromArgb(120, 200,  80) },
            { "ice",      Color.FromArgb(152, 216, 216) },
            { "fighting", Color.FromArgb(192,  48,  40) },
            { "poison",   Color.FromArgb(160,  64, 160) },
            { "ground",   Color.FromArgb(224, 192,  104) },
            { "flying",   Color.FromArgb(168, 144, 240) },
            { "psychic",  Color.FromArgb(248,  88, 136) },
            { "bug",      Color.FromArgb(168, 184,  32) },
            { "rock",     Color.FromArgb(184, 160,  56) },
            { "ghost",    Color.FromArgb(112,  88, 152) },
            { "dragon",   Color.FromArgb(112,  56, 248) },
            { "dark",     Color.FromArgb( 112,  88,  72) },
            { "steel",    Color.FromArgb(184, 184, 208) },
            { "fairy",    Color.FromArgb(238, 153, 172) },
        };

        public static Color TypeColor(string typeName)
        {
            if (typeName != null && _typeColors.TryGetValue(typeName.Trim().ToLowerInvariant(), out var color))
                return color;
            return Color.FromArgb(160, 160, 160);
        }

        // v2.1.0: simple average-channel blend of two colors, for Type Palette's dual-type swatch
        public static Color BlendColors(Color a, Color b)
        {
            return Color.FromArgb((a.R + b.R) / 2, (a.G + b.G) / 2, (a.B + b.B) / 2);
        }
    }
}

using Color = Microsoft.Maui.Graphics.Color;

namespace Rocks.Wasabee.Core.Helpers
{
    public static class WasabeeColorsHelper
    {
        public static Color GetColorFromWasabeeName(string wasabeeColorName, string defaultColorName)
        {
            if (wasabeeColorName.StartsWith("#"))
                return Color.FromArgb(wasabeeColorName);

            // Old wassabee names
            var firstConvertedName = wasabeeColorName switch
            {
                "red" => "main",
                "groupa" => "orange",
                "groupb" => "yellow",
                "groupc" => "lime",
                "groupd" => "purple",
                "groupe" => "teal",
                "groupf" => "fuchsia",
                _ => wasabeeColorName
            };

            // New wasabee names
            return firstConvertedName switch
            {
                "main" => GetColorFromWasabeeName(defaultColorName, string.Empty),
                "orange" => Color.FromArgb("#FF6600"),
                "yellow" => Color.FromArgb("#FF9900"),
                "lime" => Color.FromArgb("#BB9900"),
                "purple" => Color.FromArgb("#BB22CC"),
                "teal" => Color.FromArgb("#33CCCC"),
                "fuchsia" => Color.FromArgb("#FF55FF"),
                _ => Color.FromArgb("#FF0000")
            };
        }
    }
}

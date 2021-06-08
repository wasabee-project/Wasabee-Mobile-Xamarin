using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class WasabeeColorsHelper
    {
        public static Color GetColorFromWasabeeName(string wasabeeColorName, string defaultColorName)
        {
            if (wasabeeColorName.StartsWith("#"))
                return Color.FromHex(wasabeeColorName);

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
                "orange" => Color.FromHex("#FF6600"),
                "yellow" => Color.FromHex("#FF9900"),
                "lime" => Color.FromHex("#BB9900"),
                "purple" => Color.FromHex("#BB22CC"),
                "teal" => Color.FromHex("#33CCCC"),
                "fuchsia" => Color.FromHex("#FF55FF"),
                _ => Color.FromHex("#FF0000")
            };
        }
    }
}
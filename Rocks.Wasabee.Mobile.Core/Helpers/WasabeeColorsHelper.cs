using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class WasabeeColorsHelper
    {
        public static Color GetColorFromWasabeeName(string wasabeeColorName, string defaultColorName)
        {
            if (wasabeeColorName.StartsWith("#"))
                return Color.FromHex(wasabeeColorName);

            return wasabeeColorName switch
            {
                "main" => GetColorFromWasabeeName(defaultColorName, string.Empty),
                "groupa" => Color.FromHex("#FF6600"),
                "groupb" => Color.FromHex("#FF9900"),
                "groupc" => Color.FromHex("#BB9900"),
                "groupd" => Color.FromHex("#BB22CC"),
                "groupe" => Color.FromHex("#33CCCC"),
                "groupf" => Color.FromHex("#FF55FF"),
                _ => Color.FromHex("#FF0000")
            };
        }
    }
}
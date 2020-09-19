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
                "groupa" => Color.FromHex("#f60"),
                "groupb" => Color.FromHex("#f90"),
                "groupc" => Color.FromHex("#b90"),
                "groupd" => Color.FromHex("#b2c"),
                "groupe" => Color.FromHex("#3cc"),
                "groupf" => Color.FromHex("#f5f"),
                _ => Color.FromHex("f00")
            };
        }
    }
}
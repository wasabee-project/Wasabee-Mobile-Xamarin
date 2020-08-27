using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class WasabeeColorsHelper
    {
        public static Color GetColorFromWasabeeName(string wasabeeColorName)
        {
            return wasabeeColorName switch
            {
                "main" => Color.FromHex("f00"),
                "groupa" => Color.FromHex("#f60"),
                "groupb" => Color.FromHex("#f90"),
                "groupc" => Color.FromHex("#b90"),
                "groupd" => Color.FromHex("#b2c"),
                "groupe" => Color.FromHex("#3cc"),
                "groupf" => Color.FromHex("#f5f"),
                _ => Color.FromHex("0f0")
            };
        }
    }
}
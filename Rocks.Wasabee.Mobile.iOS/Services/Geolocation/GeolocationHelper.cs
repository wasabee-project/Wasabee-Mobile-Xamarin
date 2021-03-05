using Plugin.Geolocator;

namespace Rocks.Wasabee.Mobile.iOS.Services.Geolocation
{
    public static class GeolocationHelper
    {
        public static bool IsLocationAvailable()
        {
            if(!CrossGeolocator.IsSupported)
                return false;

            return CrossGeolocator.Current.IsGeolocationAvailable;
        }
    }

}
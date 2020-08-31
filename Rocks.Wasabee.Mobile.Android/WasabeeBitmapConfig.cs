using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using AndroidBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;
using AndroidBitmapDescriptorFactory = Android.Gms.Maps.Model.BitmapDescriptorFactory;

namespace Rocks.Wasabee.Mobile.Droid
{
    public sealed class WasabeeBitmapConfig : IBitmapDescriptorFactory
    {
        public AndroidBitmapDescriptor ToNative(BitmapDescriptor descriptor)
        {
            int iconId = 0;
            switch (descriptor.Id)
            {
                case "marker_layer_groupa":
                    iconId = Resource.Drawable.marker_layer_groupa;
                    break;
                case "marker_layer_groupb":
                    iconId = Resource.Drawable.marker_layer_groupb;
                    break;
                case "marker_layer_groupc":
                    iconId = Resource.Drawable.marker_layer_groupc;
                    break;
                case "marker_layer_groupd":
                    iconId = Resource.Drawable.marker_layer_groupd;
                    break;
                case "marker_layer_groupe":
                    iconId = Resource.Drawable.marker_layer_groupe;
                    break;
                case "marker_layer_groupf":
                    iconId = Resource.Drawable.marker_layer_groupf;
                    break;
                case "marker_layer_main":
                    iconId = Resource.Drawable.marker_layer_main;
                    break;
            }
            return AndroidBitmapDescriptorFactory.FromResource(iconId);
        }
    }
}
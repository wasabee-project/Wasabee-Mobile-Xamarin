using Android.App;
using SkiaSharp;
using SkiaSharp.Views.Android;
using System;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using AndroidBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;
using AndroidBitmapDescriptorFactory = Android.Gms.Maps.Model.BitmapDescriptorFactory;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

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
                default:
                    iconId = -999;
                    break;
            }

            if (iconId == -999)
            {
                switch (descriptor.Id)
                {
                    case "DestroyPortalAlert":
                        iconId = Resource.Drawable.wasabee_markers_destroy_pending;
                        break;
                    case "UseVirusPortalAlert":
                        iconId = Resource.Drawable.wasabee_markers_virus_pending;
                        break;
                    default:
                        throw new ArgumentException();
                }

                var svgStream = Application.Context.Resources?.OpenRawResource(iconId);
                var svg = new SKSvg();
                var picture = svg.Load(svgStream);

                return AndroidBitmapDescriptorFactory.FromBitmap(
                    SKBitmap.FromImage(
                            SKImage.FromPicture(picture, new SKSizeI((int)picture.CullRect.Size.Width, (int)picture.CullRect.Size.Height)))
                    .Resize(new SKSizeI(75, 123), SKFilterQuality.None)
                    .ToBitmap());
            }
            else
                return AndroidBitmapDescriptorFactory.FromResource(iconId);
        }
    }
}
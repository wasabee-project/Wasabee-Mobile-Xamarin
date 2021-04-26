using Rocks.Wasabee.Mobile.Core.Ui.Resources.Pins;
using SkiaSharp;
using SkiaSharp.Views.Android;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using AndroidBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;
using AndroidBitmapDescriptorFactory = Android.Gms.Maps.Model.BitmapDescriptorFactory;
using Application = Android.App.Application;

namespace Rocks.Wasabee.Mobile.Droid {
    public sealed class WasabeeBitmapConfig : IBitmapDescriptorFactory
    {
        private static readonly Dictionary<int, AndroidBitmapDescriptor> BitmapCache = new Dictionary<int, AndroidBitmapDescriptor>();

        public AndroidBitmapDescriptor ToNative(BitmapDescriptor descriptor)
        {
            switch (descriptor.Id)
            {
                case "wasabee_player_marker":
                    return CreateMarker(Resource.Drawable.wasabee_player_marker);
                case "wasabee_player_marker_self":
                    return CreateMarker(Resource.Drawable.wasabee_player_marker_self);
                case "wasabee_player_marker_gray":
                    return CreateMarker(Resource.Drawable.wasabee_player_marker_gray);
            }

            if (descriptor.Id.Contains('|'))
            {
                var descriptors = descriptor.Id.Split('|');

                var iconId = descriptors[0] switch
                {
                    "DestroyPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_destroy_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_destroy_acknowledged,
                        "completed" => Resource.Drawable.wasabee_markers_destroy_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_destroy_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "UseVirusPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_virus_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_virus_acknowledged,
                        "completed" => Resource.Drawable.wasabee_markers_virus_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_virus_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "CapturePortalMarker" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_capture_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_capture_acknowledged,
                        "completed" => Resource.Drawable.wasabee_markers_capture_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_capture_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "FarmPortalMarker" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_farm_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_farm_acknowledged,
                        "completed" => Resource.Drawable.wasabee_markers_farm_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_farm_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "LetDecayPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_decay_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_decay_acknowledged,
                        "completed" => Resource.Drawable.wasabee_markers_decay_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_decay_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "MeetAgentPortalMarker" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_meetagent_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_meetagent_acknowledged,
                        "completed" => Resource.Drawable.wasabee_markers_meetagent_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_meetagent_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "OtherPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_other_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_other_acknowledged,
                        "completed" => Resource.Drawable.wasabee_markers_other_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_other_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "RechargePortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_recharge_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_recharge_acknowledged,
                        "completed" => Resource.Drawable.wasabee_markers_recharge_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_recharge_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "UpgradePortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_upgrade_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_upgrade_acknowledged,
                        "completed" => Resource.Drawable.wasabee_markers_upgrade_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_upgrade_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "CreateLinkAlert" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_link_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_link_acknowledged,
                            "completed" => Resource.Drawable.wasabee_markers_link_completed,
                            "assigned" => Resource.Drawable.wasabee_markers_link_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "ExcludeMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_exclude_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_exclude_acknowledged,
                            "completed" => Resource.Drawable.wasabee_markers_exclude_completed,
                            "assigned" => Resource.Drawable.wasabee_markers_exclude_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "GetKeyPortalMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_key_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_key_acknowledged,
                            "completed" => Resource.Drawable.wasabee_markers_key_completed,
                            "assigned" => Resource.Drawable.wasabee_markers_key_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "GotoPortalMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_goto_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_goto_acknowledged,
                            "completed" => Resource.Drawable.wasabee_markers_goto_completed,
                            "assigned" => Resource.Drawable.wasabee_markers_goto_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    _ => throw new ArgumentOutOfRangeException(descriptors[0])
                };

                return CreateMarker(iconId);
            }

            var pinColor = descriptor.Id.StartsWith("pin_") ? 
                descriptor.Id.Substring(4) : 
                "#DD3D45"; // default goes to RED

            return CreatePin(pinColor);
        }

        private static AndroidBitmapDescriptor CreatePin(string color) => CreateBitmapFromWasabeePinSvg(color, 110, 130);
        private static AndroidBitmapDescriptor CreateMarker(int iconId) => CreateBitmapFromSvgStream(iconId, 70, 120);

        private static AndroidBitmapDescriptor CreateBitmapFromSvgStream(int iconId, int width, int height)
        {
            if (BitmapCache.ContainsKey(iconId))
                return BitmapCache[iconId];

            var svgStream = Application.Context.Resources?.OpenRawResource(iconId);
            var svg = new SkiaSharp.Extended.Svg.SKSvg();
            var picture = svg.Load(svgStream);

            var bitmap = AndroidBitmapDescriptorFactory.FromBitmap(
                SKBitmap.FromImage(
                        SKImage.FromPicture(picture, new SKSizeI((int)picture.CullRect.Size.Width, (int)picture.CullRect.Size.Height)))
                    .Resize(new SKSizeI(width, height), SKFilterQuality.High)
                    .ToBitmap());

            BitmapCache.Add(iconId, bitmap);
            return bitmap;
        }

        private static AndroidBitmapDescriptor CreateBitmapFromWasabeePinSvg(string color, int width, int height)
        {
            var colorValue = ColorConverters.FromHex(color).ToArgb();
            if (BitmapCache.ContainsKey(colorValue))
                return BitmapCache[colorValue];

            SKPicture picture;
            var pin = new WasabeePinSvg(color);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(pin.RawData)))
                picture = new SkiaSharp.Extended.Svg.SKSvg().Load(stream);

            var bitmap = AndroidBitmapDescriptorFactory.FromBitmap(
                SKBitmap.FromImage(
                        SKImage.FromPicture(picture, new SKSizeI((int)picture.CullRect.Size.Width, (int)picture.CullRect.Size.Height)))
                    .Resize(new SKSizeI(width, height), SKFilterQuality.High)
                    .ToBitmap());

            BitmapCache.Add(colorValue, bitmap);
            return bitmap;
        }
    }
}
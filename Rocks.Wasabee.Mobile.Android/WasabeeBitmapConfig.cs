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

namespace Rocks.Wasabee.Mobile.Droid
{
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
                    "DestroyPortal" => descriptors[1] switch
                    {
                        "Pending" => Resource.Drawable.wasabee_markers_destroy_pending,
                        "Acknowledged" => Resource.Drawable.wasabee_markers_destroy_acknowledged,
                        "Completed" => Resource.Drawable.wasabee_markers_destroy_completed,
                        "Assigned" => Resource.Drawable.wasabee_markers_destroy_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "UseVirus" => descriptors[1] switch
                    {
                        "Pending" => Resource.Drawable.wasabee_markers_virus_pending,
                        "Acknowledged" => Resource.Drawable.wasabee_markers_virus_acknowledged,
                        "Completed" => Resource.Drawable.wasabee_markers_virus_completed,
                        "Assigned" => Resource.Drawable.wasabee_markers_virus_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "CapturePortal" => descriptors[1] switch
                    {
                        "Pending" => Resource.Drawable.wasabee_markers_capture_pending,
                        "Acknowledged" => Resource.Drawable.wasabee_markers_capture_acknowledged,
                        "Completed" => Resource.Drawable.wasabee_markers_capture_completed,
                        "Assigned" => Resource.Drawable.wasabee_markers_capture_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "FarmPortal" => descriptors[1] switch
                    {
                        "Pending" => Resource.Drawable.wasabee_markers_farm_pending,
                        "Acknowledged" => Resource.Drawable.wasabee_markers_farm_acknowledged,
                        "Completed" => Resource.Drawable.wasabee_markers_farm_completed,
                        "Assigned" => Resource.Drawable.wasabee_markers_farm_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "LetDecay" => descriptors[1] switch
                    {
                        "Pending" => Resource.Drawable.wasabee_markers_decay_pending,
                        "Acknowledged" => Resource.Drawable.wasabee_markers_decay_acknowledged,
                        "Completed" => Resource.Drawable.wasabee_markers_decay_completed,
                        "Assigned" => Resource.Drawable.wasabee_markers_decay_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "MeetAgent" => descriptors[1] switch
                    {
                        "Pending" => Resource.Drawable.wasabee_markers_meetagent_pending,
                        "Acknowledged" => Resource.Drawable.wasabee_markers_meetagent_acknowledged,
                        "Completed" => Resource.Drawable.wasabee_markers_meetagent_completed,
                        "Assigned" => Resource.Drawable.wasabee_markers_meetagent_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "Other" => descriptors[1] switch
                    {
                        "Pending" => Resource.Drawable.wasabee_markers_other_pending,
                        "Acknowledged" => Resource.Drawable.wasabee_markers_other_acknowledged,
                        "Completed" => Resource.Drawable.wasabee_markers_other_completed,
                        "Assigned" => Resource.Drawable.wasabee_markers_other_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "RechargePortal" => descriptors[1] switch
                    {
                        "Pending" => Resource.Drawable.wasabee_markers_recharge_pending,
                        "Acknowledged" => Resource.Drawable.wasabee_markers_recharge_acknowledged,
                        "Completed" => Resource.Drawable.wasabee_markers_recharge_completed,
                        "Assigned" => Resource.Drawable.wasabee_markers_recharge_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "UpgradePortal" => descriptors[1] switch
                    {
                        "Pending" => Resource.Drawable.wasabee_markers_upgrade_pending,
                        "Acknowledged" => Resource.Drawable.wasabee_markers_upgrade_acknowledged,
                        "Completed" => Resource.Drawable.wasabee_markers_upgrade_completed,
                        "Assigned" => Resource.Drawable.wasabee_markers_upgrade_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "CreateLink" =>
                        descriptors[1] switch
                        {
                            "Pending" => Resource.Drawable.wasabee_markers_link_pending,
                            "Acknowledged" => Resource.Drawable.wasabee_markers_link_acknowledged,
                            "Completed" => Resource.Drawable.wasabee_markers_link_completed,
                            "Assigned" => Resource.Drawable.wasabee_markers_link_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "Exclude" =>
                        descriptors[1] switch
                        {
                            "Pending" => Resource.Drawable.wasabee_markers_exclude_pending,
                            "Acknowledged" => Resource.Drawable.wasabee_markers_exclude_acknowledged,
                            "Completed" => Resource.Drawable.wasabee_markers_exclude_completed,
                            "Assigned" => Resource.Drawable.wasabee_markers_exclude_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "GetKey" =>
                        descriptors[1] switch
                        {
                            "Pending" => Resource.Drawable.wasabee_markers_key_pending,
                            "Acknowledged" => Resource.Drawable.wasabee_markers_key_acknowledged,
                            "Completed" => Resource.Drawable.wasabee_markers_key_completed,
                            "Assigned" => Resource.Drawable.wasabee_markers_key_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "GoToPortal" =>
                        descriptors[1] switch
                        {
                            "Pending" => Resource.Drawable.wasabee_markers_goto_pending,
                            "Acknowledged" => Resource.Drawable.wasabee_markers_goto_acknowledged,
                            "Completed" => Resource.Drawable.wasabee_markers_goto_completed,
                            "Assigned" => Resource.Drawable.wasabee_markers_goto_assigned,
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
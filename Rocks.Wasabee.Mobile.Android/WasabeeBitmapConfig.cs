using Android.App;
using SkiaSharp;
using SkiaSharp.Views.Android;
using System;
using System.Collections.Generic;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using AndroidBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;
using AndroidBitmapDescriptorFactory = Android.Gms.Maps.Model.BitmapDescriptorFactory;

namespace Rocks.Wasabee.Mobile.Droid
{
    public sealed class WasabeeBitmapConfig : IBitmapDescriptorFactory
    {
        private static Dictionary<int, AndroidBitmapDescriptor> BitmapCache = new Dictionary<int, AndroidBitmapDescriptor>();

        public AndroidBitmapDescriptor ToNative(BitmapDescriptor descriptor)
        {
            int iconId;
            if (descriptor.Id.Equals("wasabee_player_marker"))
            {
                return CreateMarker(Resource.Drawable.wasabee_player_marker);
            }
            
            if (descriptor.Id.Equals("wasabee_player_marker_self"))
            {
                return CreateMarker(Resource.Drawable.wasabee_player_marker_self);
            }
            
            if (descriptor.Id.Equals("wasabee_player_marker_gray"))
            {
                return CreateMarker(Resource.Drawable.wasabee_player_marker_gray);
            }

            if (descriptor.Id.Contains('|'))
            {
                var descriptors = descriptor.Id.Split('|');

                iconId = descriptors[0] switch
                {
                    "DestroyPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_destroy_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_destroy_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_destroy_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_destroy_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "UseVirusPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_virus_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_virus_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_virus_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_virus_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "CapturePortalMarker" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_capture_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_capture_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_capture_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_capture_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "FarmPortalMarker" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_farm_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_farm_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_farm_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_farm_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "LetDecayPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_decay_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_decay_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_decay_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_decay_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "MeetAgentPortalMarker" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_meetagent_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_meetagent_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_meetagent_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_meetagent_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "OtherPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_other_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_other_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_other_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_other_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "RechargePortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_recharge_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_recharge_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_recharge_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_recharge_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "UpgradePortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_upgrade_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_upgrade_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_upgrade_completed,
                        "assigned" => Resource.Drawable.wasabee_markers_upgrade_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "CreateLinkAlert" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_link_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_link_acknowledge,
                            "completed" => Resource.Drawable.wasabee_markers_link_completed,
                            "assigned" => Resource.Drawable.wasabee_markers_link_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "ExcludeMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_exclude_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_exclude_acknowledge,
                            "completed" => Resource.Drawable.wasabee_markers_exclude_completed,
                            "assigned" => Resource.Drawable.wasabee_markers_exclude_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "GetKeyPortalMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_key_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_key_acknowledge,
                            "completed" => Resource.Drawable.wasabee_markers_key_completed,
                            "assigned" => Resource.Drawable.wasabee_markers_key_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "GotoPortalMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_goto_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_goto_acknowledge,
                            "completed" => Resource.Drawable.wasabee_markers_goto_completed,
                            "assigned" => Resource.Drawable.wasabee_markers_goto_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    _ => throw new ArgumentOutOfRangeException(descriptors[0])
                };

                return CreateMarker(iconId);
            }

            if (descriptor.Id.Contains("#"))
                iconId = Resource.Drawable.pin_green;
            else
                iconId = descriptor.Id switch
                {
                    "pin_orange" => Resource.Drawable.pin_orange,
                    "pin_yellow" => Resource.Drawable.pin_yellow,
                    "pin_tan" => Resource.Drawable.pin_tan,
                    "pin_purple" => Resource.Drawable.pin_purple,
                    "pin_teal" => Resource.Drawable.pin_teal,
                    "pin_fuschia" => Resource.Drawable.pin_fuschia,
                    "pin_red" => Resource.Drawable.pin_red,
                    _ => Resource.Drawable.pin_lime
                };

            return CreatePin(iconId);
        }

        private static AndroidBitmapDescriptor CreatePin(int iconId) => CreateBitmapFromSvgStream(iconId, 60, 120);
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
                    .Resize(new SKSizeI(width, height), SKFilterQuality.None)
                    .ToBitmap());

            BitmapCache.Add(iconId, bitmap);
            return bitmap;
        }
    }
}
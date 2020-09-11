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
            int iconId;
            if (descriptor.Id.Equals("wasabee_player_marker"))
            {
                return CreateBitmapFromSvgStream(Resource.Drawable.wasabee_player_marker);
            }
            else if (descriptor.Id.Contains('|'))
            {
                var descriptors = descriptor.Id.Split('|');

                iconId = descriptors[0] switch
                {
                    "DestroyPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_destroy_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_destroy_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_destroy_done,
                        "assigned" => Resource.Drawable.wasabee_markers_destroy_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[0])
                    },
                    "UseVirusPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_virus_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_virus_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_virus_done,
                        "assigned" => Resource.Drawable.wasabee_markers_virus_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "CapturePortalMarker" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_capture_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_capture_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_capture_done,
                        "assigned" => Resource.Drawable.wasabee_markers_capture_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "FarmPortalMarker" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_farm_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_farm_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_farm_done,
                        "assigned" => Resource.Drawable.wasabee_markers_farm_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "LetDecayPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_decay_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_decay_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_decay_done,
                        "assigned" => Resource.Drawable.wasabee_markers_decay_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "MeetAgentPortalMarker" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_meetagent_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_meetagent_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_meetagent_done,
                        "assigned" => Resource.Drawable.wasabee_markers_meetagent_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "OtherPortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_other_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_other_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_other_done,
                        "assigned" => Resource.Drawable.wasabee_markers_other_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "RechargePortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_recharge_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_recharge_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_recharge_done,
                        "assigned" => Resource.Drawable.wasabee_markers_recharge_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "UpgradePortalAlert" => descriptors[1] switch
                    {
                        "pending" => Resource.Drawable.wasabee_markers_upgrade_pending,
                        "acknowledged" => Resource.Drawable.wasabee_markers_upgrade_acknowledge,
                        "completed" => Resource.Drawable.wasabee_markers_upgrade_done,
                        "assigned" => Resource.Drawable.wasabee_markers_upgrade_assigned,
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "CreateLinkAlert" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_link_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_link_acknowledge,
                            "completed" => Resource.Drawable.wasabee_markers_link_done,
                            "assigned" => Resource.Drawable.wasabee_markers_link_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "ExcludeMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_exclude_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_exclude_acknowledge,
                            "completed" => Resource.Drawable.wasabee_markers_exclude_done,
                            "assigned" => Resource.Drawable.wasabee_markers_exclude_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "GetKeyPortalMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_key_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_key_acknowledge,
                            "completed" => Resource.Drawable.wasabee_markers_key_done,
                            "assigned" => Resource.Drawable.wasabee_markers_key_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "GotoPortalMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => Resource.Drawable.wasabee_markers_goto_pending,
                            "acknowledged" => Resource.Drawable.wasabee_markers_goto_acknowledge,
                            "completed" => Resource.Drawable.wasabee_markers_goto_done,
                            "assigned" => Resource.Drawable.wasabee_markers_goto_assigned,
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    _ => throw new ArgumentOutOfRangeException(descriptors[0])
                };

                return CreateBitmapFromSvgStream(iconId);
            }
            else
            {
                iconId = descriptor.Id switch
                {
                    "marker_layer_groupa" => Resource.Drawable.marker_layer_groupa,
                    "marker_layer_groupb" => Resource.Drawable.marker_layer_groupb,
                    "marker_layer_groupc" => Resource.Drawable.marker_layer_groupc,
                    "marker_layer_groupd" => Resource.Drawable.marker_layer_groupd,
                    "marker_layer_groupe" => Resource.Drawable.marker_layer_groupe,
                    "marker_layer_groupf" => Resource.Drawable.marker_layer_groupf,
                    "marker_layer_main" => Resource.Drawable.marker_layer_main,
                    _ => throw new ArgumentOutOfRangeException(descriptor.Id)
                };

                return AndroidBitmapDescriptorFactory.FromResource(iconId);
            }
        }

        private AndroidBitmapDescriptor CreateBitmapFromSvgStream(int iconId)
        {
            var svgStream = Application.Context.Resources?.OpenRawResource(iconId);
            var svg = new SKSvg();
            var picture = svg.Load(svgStream);

            return AndroidBitmapDescriptorFactory.FromBitmap(
                SKBitmap.FromImage(
                        SKImage.FromPicture(picture, new SKSizeI((int)picture.CullRect.Size.Width, (int)picture.CullRect.Size.Height)))
                    .Resize(new SKSizeI(70, 120), SKFilterQuality.None)
                    .ToBitmap());
        }
    }
}
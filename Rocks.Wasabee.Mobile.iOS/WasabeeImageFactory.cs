using System;
using Foundation;
using UIKit;
using Xamarin;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.iOS.Factories;

namespace Rocks.Wasabee.Mobile.iOS
{
    public class WasabeeImageFactory : IImageFactory
    {
        public UIImage ToUIImage(BitmapDescriptor descriptor)
        {
            if (descriptor.Id.Equals("wasabee_player_marker"))
            {
                return CreateMarker("wasabee_player_marker.svg");
            }

            string fileName;
            if (descriptor.Id.Contains('|'))
            {
                var descriptors = descriptor.Id.Split('|');
                fileName = "markers." + descriptors[0] switch
                {
                    "DestroyPortalAlert" => descriptors[1] switch
                    {
                        "pending" => "wasabee_markers_destroy_pending",
                        "acknowledged" => "wasabee_markers_destroy_acknowledge",
                        "completed" => "wasabee_markers_destroy_done",
                        "assigned" => "wasabee_markers_destroy_assigned",
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "UseVirusPortalAlert" => descriptors[1] switch
                    {
                        "pending" => "wasabee_markers_virus_pending",
                        "acknowledged" => "wasabee_markers_virus_acknowledge",
                        "completed" => "wasabee_markers_virus_done",
                        "assigned" => "wasabee_markers_virus_assigned",
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "CapturePortalMarker" => descriptors[1] switch
                    {
                        "pending" => "wasabee_markers_capture_pending",
                        "acknowledged" => "wasabee_markers_capture_acknowledge",
                        "completed" => "wasabee_markers_capture_done",
                        "assigned" => "wasabee_markers_capture_assigned",
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "FarmPortalMarker" => descriptors[1] switch
                    {
                        "pending" => "wasabee_markers_farm_pending",
                        "acknowledged" => "wasabee_markers_farm_acknowledge",
                        "completed" => "wasabee_markers_farm_done",
                        "assigned" => "wasabee_markers_farm_assigned",
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "LetDecayPortalAlert" => descriptors[1] switch
                    {
                        "pending" => "wasabee_markers_decay_pending",
                        "acknowledged" => "wasabee_markers_decay_acknowledge",
                        "completed" => "wasabee_markers_decay_done",
                        "assigned" => "wasabee_markers_decay_assigned",
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "MeetAgentPortalMarker" => descriptors[1] switch
                    {
                        "pending" => "wasabee_markers_meetagent_pending",
                        "acknowledged" => "wasabee_markers_meetagent_acknowledge",
                        "completed" => "wasabee_markers_meetagent_done",
                        "assigned" => "wasabee_markers_meetagent_assigned",
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "OtherPortalAlert" => descriptors[1] switch
                    {
                        "pending" => "wasabee_markers_other_pending",
                        "acknowledged" => "wasabee_markers_other_acknowledge",
                        "completed" => "wasabee_markers_other_done",
                        "assigned" => "wasabee_markers_other_assigned",
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "RechargePortalAlert" => descriptors[1] switch
                    {
                        "pending" => "wasabee_markers_recharge_pending",
                        "acknowledged" => "wasabee_markers_recharge_acknowledge",
                        "completed" => "wasabee_markers_recharge_done",
                        "assigned" => "wasabee_markers_recharge_assigned",
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "UpgradePortalAlert" => descriptors[1] switch
                    {
                        "pending" => "wasabee_markers_upgrade_pending",
                        "acknowledged" => "wasabee_markers_upgrade_acknowledge",
                        "completed" => "wasabee_markers_upgrade_done",
                        "assigned" => "wasabee_markers_upgrade_assigned",
                        _ => throw new ArgumentOutOfRangeException(descriptors[1])
                    },
                    "CreateLinkAlert" =>
                        descriptors[1] switch
                        {
                            "pending" => "wasabee_markers_link_pending",
                            "acknowledged" => "wasabee_markers_link_acknowledge",
                            "completed" => "wasabee_markers_link_done",
                            "assigned" => "wasabee_markers_link_assigned",
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "ExcludeMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => "wasabee_markers_exclude_pending",
                            "acknowledged" => "wasabee_markers_exclude_acknowledge",
                            "completed" => "wasabee_markers_exclude_done",
                            "assigned" => "wasabee_markers_exclude_assigned",
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "GetKeyPortalMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => "wasabee_markers_key_pending",
                            "acknowledged" => "wasabee_markers_key_acknowledge",
                            "completed" => "wasabee_markers_key_done",
                            "assigned" => "wasabee_markers_key_assigned",
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    "GotoPortalMarker" =>
                        descriptors[1] switch
                        {
                            "pending" => "wasabee_markers_goto_pending",
                            "acknowledged" => "wasabee_markers_goto_acknowledge",
                            "completed" => "wasabee_markers_goto_done",
                            "assigned" => "wasabee_markers_goto_assigned",
                            _ => throw new ArgumentOutOfRangeException(descriptors[1])
                        },
                    _ => throw new ArgumentOutOfRangeException(descriptors[0])
                } + ".svg";

                return CreateMarker(fileName);
            }
            else
            {
                fileName = descriptor.Id;
                return CreatePin($"pins.{fileName}.svg");
            }
        }
        
        private static UIImage CreatePin(string resourcePath) => CreateUiImageFromSvgStream(resourcePath, 60, 120);
        private static UIImage CreateMarker(string resourcePath) => CreateUiImageFromSvgStream(resourcePath, 70, 120);

        private static UIImage CreateUiImageFromSvgStream(string resourcePath, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                return Google.Maps.Marker.MarkerImage(UIColor.Red);

            return UIImage.FromBundle(resourcePath);
        }
    }
}
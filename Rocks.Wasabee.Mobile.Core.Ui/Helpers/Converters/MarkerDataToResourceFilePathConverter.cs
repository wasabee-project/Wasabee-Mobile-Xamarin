using Rocks.Wasabee.Mobile.Core.Models.Operations;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui.Helpers.Converters
{
    public class MarkerDataToResourceFilePathConverter : IValueConverter
    {
        private static string DefaultMarker => Device.RuntimePlatform == Device.Android ? "wasabee_markers_other_pending.svg" : "wasabee/markers/other/pending.svg";
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, null))
                return DefaultMarker;
            try
            {
                if (value is MarkerModel marker)
                {
                    var type = marker.Type;
                    var state = marker.State;
                    var svgFilename = type switch
                    {
                        "DestroyPortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_destroy_pending.svg",
                            "acknowledged" => "wasabee_markers_destroy_acknowledged.svg",
                            "completed" => "wasabee_markers_destroy_completed.svg",
                            "assigned" => "wasabee_markers_destroy_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "UseVirusPortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_virus_pending.svg",
                            "acknowledged" => "wasabee_markers_virus_acknowledged.svg",
                            "completed" => "wasabee_markers_virus_completed.svg",
                            "assigned" => "wasabee_markers_virus_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "CapturePortalMarker" => state switch
                        {
                            "pending" => "wasabee_markers_capture_pending.svg",
                            "acknowledged" => "wasabee_markers_capture_acknowledged.svg",
                            "completed" => "wasabee_markers_capture_completed.svg",
                            "assigned" => "wasabee_markers_capture_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "FarmPortalMarker" => state switch
                        {
                            "pending" => "wasabee_markers_farm_pending.svg",
                            "acknowledged" => "wasabee_markers_farm_acknowledged.svg",
                            "completed" => "wasabee_markers_farm_completed.svg",
                            "assigned" => "wasabee_markers_farm_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "LetDecayPortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_decay_pending.svg",
                            "acknowledged" => "wasabee_markers_decay_acknowledged.svg",
                            "completed" => "wasabee_markers_decay_completed.svg",
                            "assigned" => "wasabee_markers_decay_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "MeetAgentPortalMarker" => state switch
                        {
                            "pending" => "wasabee_markers_meetagent_pending.svg",
                            "acknowledged" => "wasabee_markers_meetagent_acknowledged.svg",
                            "completed" => "wasabee_markers_meetagent_completed.svg",
                            "assigned" => "wasabee_markers_meetagent_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "OtherPortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_other_pending.svg",
                            "acknowledged" => "wasabee_markers_other_acknowledged.svg",
                            "completed" => "wasabee_markers_other_completed.svg",
                            "assigned" => "wasabee_markers_other_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "RechargePortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_recharge_pending.svg",
                            "acknowledged" => "wasabee_markers_recharge_acknowledged.svg",
                            "completed" => "wasabee_markers_recharge_completed.svg",
                            "assigned" => "wasabee_markers_recharge_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "UpgradePortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_upgrade_pending.svg",
                            "acknowledged" => "wasabee_markers_upgrade_acknowledged.svg",
                            "completed" => "wasabee_markers_upgrade_completed.svg",
                            "assigned" => "wasabee_markers_upgrade_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "CreateLinkAlert" => state switch
                        {
                            "pending" => "wasabee_markers_link_pending.svg",
                            "acknowledged" => "wasabee_markers_link_acknowledged.svg",
                            "completed" => "wasabee_markers_link_completed.svg",
                            "assigned" => "wasabee_markers_link_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "ExcludeMarker" => state switch
                        {
                            "pending" => "wasabee_markers_exclude_pending.svg",
                            "acknowledged" => "wasabee_markers_exclude_acknowledged.svg",
                            "completed" => "wasabee_markers_exclude_completed.svg",
                            "assigned" => "wasabee_markers_exclude_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "GetKeyPortalMarker" => state switch
                        {
                            "pending" => "wasabee_markers_key_pending.svg",
                            "acknowledged" => "wasabee_markers_key_acknowledged.svg",
                            "completed" => "wasabee_markers_key_completed.svg",
                            "assigned" => "wasabee_markers_key_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "GotoPortalMarker" => state switch
                        {
                            "pending" => "wasabee_markers_goto_pending.svg",
                            "acknowledged" => "wasabee_markers_goto_acknowledged.svg",
                            "completed" => "wasabee_markers_goto_completed.svg",
                            "assigned" => "wasabee_markers_goto_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        _ => throw new ArgumentOutOfRangeException(type)
                    };

                    return Device.RuntimePlatform == Device.Android ? svgFilename : svgFilename.Replace('_', '/').Replace(".svg", ".png");

                }
                else
                    return DefaultMarker;
            }
            catch (Exception)
            {
                return DefaultMarker;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
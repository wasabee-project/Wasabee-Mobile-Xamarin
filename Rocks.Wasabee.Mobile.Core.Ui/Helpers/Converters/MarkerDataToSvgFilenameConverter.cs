using Rocks.Wasabee.Mobile.Core.Models.Operations;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui.Helpers.Converters
{
    public class MarkerDataToSvgFilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, null))
                return "wasabee_markers_other_pending.svg";
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
                            "acknowledged" => "wasabee_markers_destroy_acknowledge.svg",
                            "completed" => "wasabee_markers_destroy_done.svg",
                            "assigned" => "wasabee_markers_destroy_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "UseVirusPortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_virus_pending.svg",
                            "acknowledged" => "wasabee_markers_virus_acknowledge.svg",
                            "completed" => "wasabee_markers_virus_done.svg",
                            "assigned" => "wasabee_markers_virus_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "CapturePortalMarker" => state switch
                        {
                            "pending" => "wasabee_markers_capture_pending.svg",
                            "acknowledged" => "wasabee_markers_capture_acknowledge.svg",
                            "completed" => "wasabee_markers_capture_done.svg",
                            "assigned" => "wasabee_markers_capture_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "FarmPortalMarker" => state switch
                        {
                            "pending" => "wasabee_markers_farm_pending.svg",
                            "acknowledged" => "wasabee_markers_farm_acknowledge.svg",
                            "completed" => "wasabee_markers_farm_done.svg",
                            "assigned" => "wasabee_markers_farm_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "LetDecayPortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_decay_pending.svg",
                            "acknowledged" => "wasabee_markers_decay_acknowledge.svg",
                            "completed" => "wasabee_markers_decay_done.svg",
                            "assigned" => "wasabee_markers_decay_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "MeetAgentPortalMarker" => state switch
                        {
                            "pending" => "wasabee_markers_meetagent_pending.svg",
                            "acknowledged" => "wasabee_markers_meetagent_acknowledge.svg",
                            "completed" => "wasabee_markers_meetagent_done.svg",
                            "assigned" => "wasabee_markers_meetagent_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "OtherPortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_other_pending.svg",
                            "acknowledged" => "wasabee_markers_other_acknowledge.svg",
                            "completed" => "wasabee_markers_other_done.svg",
                            "assigned" => "wasabee_markers_other_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "RechargePortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_recharge_pending.svg",
                            "acknowledged" => "wasabee_markers_recharge_acknowledge.svg",
                            "completed" => "wasabee_markers_recharge_done.svg",
                            "assigned" => "wasabee_markers_recharge_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "UpgradePortalAlert" => state switch
                        {
                            "pending" => "wasabee_markers_upgrade_pending.svg",
                            "acknowledged" => "wasabee_markers_upgrade_acknowledge.svg",
                            "completed" => "wasabee_markers_upgrade_done.svg",
                            "assigned" => "wasabee_markers_upgrade_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "CreateLinkAlert" => state switch
                        {
                            "pending" => "wasabee_markers_link_pending.svg",
                            "acknowledged" => "wasabee_markers_link_acknowledge.svg",
                            "completed" => "wasabee_markers_link_done.svg",
                            "assigned" => "wasabee_markers_link_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "ExcludeMarker" => state switch
                        {
                            "pending" => "wasabee_markers_exclude_pending.svg",
                            "acknowledged" => "wasabee_markers_exclude_acknowledge.svg",
                            "completed" => "wasabee_markers_exclude_done.svg",
                            "assigned" => "wasabee_markers_exclude_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "GetKeyPortalMarker" => state switch
                        {
                            "pending" => "wasabee_markers_key_pending.svg",
                            "acknowledged" => "wasabee_markers_key_acknowledge.svg",
                            "completed" => "wasabee_markers_key_done.svg",
                            "assigned" => "wasabee_markers_key_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        "GotoPortalMarker" => state switch
                        {
                            "pending" => "wasabee_markers_goto_pending.svg",
                            "acknowledged" => "wasabee_markers_goto_acknowledge.svg",
                            "completed" => "wasabee_markers_goto_done.svg",
                            "assigned" => "wasabee_markers_goto_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException(state)
                        },
                        _ => throw new ArgumentOutOfRangeException(type)
                    };

                    return svgFilename;

                }
                else
                    return "wasabee_markers_other_pending.svg";
            }
            catch (Exception)
            {
                return "wasabee_markers_other_pending.svg";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
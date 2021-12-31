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
                        MarkerType.DestroyPortal => state switch
                        {
                            TaskState.Pending => "wasabee_markers_destroy_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_destroy_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_destroy_completed.svg",
                            TaskState.Assigned => "wasabee_markers_destroy_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.UseVirus => state switch
                        {
                            TaskState.Pending => "wasabee_markers_virus_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_virus_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_virus_completed.svg",
                            TaskState.Assigned => "wasabee_markers_virus_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.CapturePortal => state switch
                        {
                            TaskState.Pending => "wasabee_markers_capture_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_capture_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_capture_completed.svg",
                            TaskState.Assigned => "wasabee_markers_capture_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.FarmPortal => state switch
                        {
                            TaskState.Pending => "wasabee_markers_farm_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_farm_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_farm_completed.svg",
                            TaskState.Assigned => "wasabee_markers_farm_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.LetDecay => state switch
                        {
                            TaskState.Pending => "wasabee_markers_decay_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_decay_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_decay_completed.svg",
                            TaskState.Assigned => "wasabee_markers_decay_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.MeetAgent => state switch
                        {
                            TaskState.Pending => "wasabee_markers_meetagent_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_meetagent_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_meetagent_completed.svg",
                            TaskState.Assigned => "wasabee_markers_meetagent_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.Other => state switch
                        {
                            TaskState.Pending => "wasabee_markers_other_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_other_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_other_completed.svg",
                            TaskState.Assigned => "wasabee_markers_other_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.RechargePortal => state switch
                        {
                            TaskState.Pending => "wasabee_markers_recharge_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_recharge_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_recharge_completed.svg",
                            TaskState.Assigned => "wasabee_markers_recharge_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.UpgradePortal => state switch
                        {
                            TaskState.Pending => "wasabee_markers_upgrade_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_upgrade_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_upgrade_completed.svg",
                            TaskState.Assigned => "wasabee_markers_upgrade_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.CreateLink => state switch
                        {
                            TaskState.Pending => "wasabee_markers_link_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_link_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_link_completed.svg",
                            TaskState.Assigned => "wasabee_markers_link_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.Exclude => state switch
                        {
                            TaskState.Pending => "wasabee_markers_exclude_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_exclude_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_exclude_completed.svg",
                            TaskState.Assigned => "wasabee_markers_exclude_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.GetKey => state switch
                        {
                            TaskState.Pending => "wasabee_markers_key_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_key_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_key_completed.svg",
                            TaskState.Assigned => "wasabee_markers_key_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        MarkerType.GoToPortal => state switch
                        {
                            TaskState.Pending => "wasabee_markers_goto_pending.svg",
                            TaskState.Acknowledged => "wasabee_markers_goto_acknowledged.svg",
                            TaskState.Completed => "wasabee_markers_goto_completed.svg",
                            TaskState.Assigned => "wasabee_markers_goto_assigned.svg",
                            _ => throw new ArgumentOutOfRangeException($"{type} : {state}")
                        },
                        _ => throw new ArgumentOutOfRangeException(type.ToString())
                    };

                    return Device.RuntimePlatform == Device.Android ? svgFilename : svgFilename.Replace('_', '/').Replace(".svg", ".png");

                }

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
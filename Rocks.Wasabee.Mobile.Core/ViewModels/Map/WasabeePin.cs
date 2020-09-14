using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Xamarin.Forms.GoogleMaps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Map
{
    public class WasabeePin
    {
        public Pin Pin { get; }

        public WasabeePin(Pin pin)
        {
            Pin = pin;

            if (Pin.Label == null)
                Pin.Label = string.Empty;
        }

        public PortalModel Portal { get; set; }
        public MarkerModel Marker { get; set; }
        public string AssignedTo { get; set; }
    }

    public class WasabeePlayerPin
    {
        public Pin Pin { get; }

        public WasabeePlayerPin(Pin pin)
        {
            Pin = pin;

            if (Pin.Label == null)
                Pin.Label = "Unknown player";
        }

        public string AgentName { get; set; }
        public string TimeAgo { get; set; }
    }
}
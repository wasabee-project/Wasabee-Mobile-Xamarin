using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Xamarin.Forms.GoogleMaps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation
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

        public PortalModel Portal { get; set; } = new PortalModel();
        public MarkerModel Marker { get; set; } = new MarkerModel();
        public string AssignedTo { get; set; }
        public bool HasComment => !string.IsNullOrWhiteSpace(Portal?.Comment) ||
                                  !string.IsNullOrWhiteSpace(Portal?.Hardness) ||
                                  !string.IsNullOrWhiteSpace(Marker?.Comment);
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
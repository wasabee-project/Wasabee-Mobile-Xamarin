using Xamarin.Forms.GoogleMaps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation.MapElements
{
    public class WasabeeAgentPin
    {
        public Pin Pin { get; }

        public WasabeeAgentPin(Pin pin)
        {
            Pin = pin;

            if (Pin.Label == null)
                Pin.Label = "Unknown player";
        }

        public string AgentId { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
    }
}
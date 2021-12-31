using Rocks.Wasabee.Mobile.Core.Models.Agent;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using System.Collections.Generic;
using Xamarin.Forms.GoogleMaps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation.MapElements
{
    public class WasabeePin
    {
        public Pin Pin { get; }

        public WasabeePin(Pin pin)
        {
            Pin = pin;
            Pin.Label ??= string.Empty;
        }

        public PortalModel Portal { get; set; } = new PortalModel();
        public MarkerModel Marker { get; set; } = new MarkerModel();
        public List<AgentModel> Assignments { get; set; } = new List<AgentModel>();
        public bool HasComment => !string.IsNullOrWhiteSpace(Portal.Comment) ||
                                  !string.IsNullOrWhiteSpace(Portal.Hardness) ||
                                  !string.IsNullOrWhiteSpace(Marker.Comment);
    }
}
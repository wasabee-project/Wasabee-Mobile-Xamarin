using Maui.GoogleMaps;

namespace Rocks.Wasabee.Core.ViewModels.Operation.MapElements
{
    public class WasabeeLink
    {
        public Polyline Polyline { get; }

        public WasabeeLink(Polyline polyline)
        {
            Polyline = polyline;
        }

        public string LinkId { get; set; } = string.Empty;
    }
}
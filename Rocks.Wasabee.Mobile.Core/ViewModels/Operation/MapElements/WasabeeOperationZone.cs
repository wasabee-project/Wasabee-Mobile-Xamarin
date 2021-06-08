using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation.MapElements
{
    public class WasabeeOperationZone
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public Polygon Polygon { get; set; } = new Polygon();

        public WasabeeOperationZone(string name)
        {
            Name = name;
        }
    }
}
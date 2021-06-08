using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation.MapElements
{
    public class WasabeeOperationZone
    {
        public string Name { get; }
        public Polygon Polygon { get; } = new Polygon() { StrokeWidth = 2, IsClickable = false };

        public WasabeeOperationZone(string name)
        {
            Name = name;
        }
        
        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;

                Polygon.StrokeColor = Color;
                Polygon.FillColor = Color.MultiplyAlpha(0.15);
            }
        }
    }
}
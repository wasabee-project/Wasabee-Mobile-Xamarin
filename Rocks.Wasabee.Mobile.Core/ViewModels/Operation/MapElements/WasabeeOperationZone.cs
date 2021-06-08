using MvvmCross.ViewModels;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation.MapElements
{
    public class WasabeeOperationZone
    {
        public string Name { get; set; } = string.Empty;
        public Color Color { get; set; }
        public MvxObservableCollection<ZonePoint> Points { get; set; } = new MvxObservableCollection<ZonePoint>();
    }

    public class ZonePoint
    {
        public int Position { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
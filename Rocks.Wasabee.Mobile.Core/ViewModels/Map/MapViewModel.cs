using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Map
{
    public class MapViewModel : BaseViewModel
    {
        public MapViewModel()
        {

        }

        public override async Task Initialize()
        {
            await base.Initialize();

            MapElements = new List<MapElement>()
            {
                new Polyline
                {
                    StrokeColor = Color.Black,
                    StrokeWidth = 12,
                    Geopath =
                    {
                        new Position(47.6381401, -122.1317367),
                        new Position(47.6381473, -122.1350841),
                        new Position(47.6382847, -122.1353094),
                        new Position(47.6384582, -122.1354703),
                        new Position(47.6401136, -122.1360819),
                        new Position(47.6403883, -122.1364681),
                        new Position(47.6407426, -122.1377019),
                        new Position(47.6412558, -122.1404056),
                        new Position(47.6414148, -122.1418647),
                        new Position(47.6414654, -122.1432702)
                    }
                },

                new Polygon
                {
                    StrokeColor = Color.FromHex("#1BA1E2"),
                    StrokeWidth = 8,
                    FillColor = Color.FromHex("#881BA1E2"),
                    Geopath =
                    {
                        new Position(47.6368678, -122.137305),
                        new Position(47.6368894, -122.134655),
                        new Position(47.6359424, -122.134655),
                        new Position(47.6359496, -122.1325521),
                        new Position(47.6424124, -122.1325199),
                        new Position(47.642463, -122.1338932),
                        new Position(47.6406414, -122.1344833),
                        new Position(47.6384943, -122.1361248),
                        new Position(47.6372943, -122.1376912),
                        new Position(47.6368678, -122.137305),
                    }
                }
            };
        }

        public List<MapElement> MapElements { get; set; } = new List<MapElement>();
        public MapSpan MapRegion { get; set; } = MapSpan.FromCenterAndRadius(new Position(47.640663, -122.1376177), Distance.FromKilometers(5));
    }
}
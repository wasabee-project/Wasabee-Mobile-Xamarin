using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Map
{
    public class MapViewModel : BaseViewModel
    {
        private readonly OperationsDatabase _operationsDatabase;

        public MapViewModel(OperationsDatabase operationsDatabase)
        {
            _operationsDatabase = operationsDatabase;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            OperationsList = await _operationsDatabase.GetOperationModels();

            var firstOp = OperationsList.FirstOrDefault(x => x.Name.Equals("Wasabee Dev"));
            if (firstOp != null)
            {
                foreach (var link in firstOp.Links)
                {
                    var fromPortal = firstOp.Portals.First(x => x.Id.Equals(link.FromPortalId));
                    var toPortal = firstOp.Portals.First(x => x.Id.Equals(link.ToPortalId));

                    var culture = CultureInfo.GetCultureInfo("en-US");
                    try
                    {
                        double.TryParse(fromPortal.Lat, NumberStyles.Float, culture, out double fromLat);
                        double.TryParse(fromPortal.Lng, NumberStyles.Float, culture, out double fromLng);
                        double.TryParse(toPortal.Lat, NumberStyles.Float, culture, out double toLat);
                        double.TryParse(toPortal.Lng, NumberStyles.Float, culture, out double toLng);

                        MapElements.Add(
                            new Polyline()
                            {
                                StrokeColor = Color.Orange,
                                StrokeWidth = 2,
                                Geopath =
                                {
                                    new Position(fromLat, fromLng),
                                    new Position(toLat, toLng)
                                }
                            });

                        Pins.Add(new Pin() { Position = new Position(fromLat, fromLng), Label = fromPortal.Name });
                        Pins.Add(new Pin() { Position = new Position(toLat, toLng), Label = toPortal.Name });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                var portal = firstOp.Portals.First();
                MapRegion = MapSpan.FromCenterAndRadius(Pins.First().Position, Distance.FromKilometers(5));
            }
        }

        public List<OperationModel> OperationsList { get; set; } = new List<OperationModel>();
        public List<MapElement> MapElements { get; set; } = new List<MapElement>();
        public List<Pin> Pins { get; set; } = new List<Pin>();
        public MapSpan MapRegion { get; set; } = MapSpan.FromCenterAndRadius(new Position(47.640663, -122.1376177), Distance.FromKilometers(5));
    }
}
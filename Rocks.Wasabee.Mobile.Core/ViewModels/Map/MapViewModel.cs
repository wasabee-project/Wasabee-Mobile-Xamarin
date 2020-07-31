using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using System.Collections.Generic;
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

            var firstOp = OperationsList.FirstOrDefault();
            if (firstOp != null)
            {
                foreach (var link in firstOp.Links)
                {
                    var fromPortal = firstOp.Portals.First(x => x.Id.Equals(link.FromPortalId));
                    var toPortal = firstOp.Portals.First(x => x.Id.Equals(link.ToPortalId));

                    MapElements.Add(
                        new Polyline()
                        {
                            StrokeColor = Color.Black,
                            StrokeWidth = 12,
                            Geopath =
                            {
                                new Position(double.Parse(fromPortal.Lat), double.Parse(fromPortal.Lng)),
                                new Position(double.Parse(toPortal.Lat), double.Parse(toPortal.Lng)),
                            }
                        });
                }

                var portal = firstOp.Portals.First();
                MapRegion = MapSpan.FromCenterAndRadius(new Position(double.Parse(portal.Lat), double.Parse(portal.Lng)), Distance.FromKilometers(5));
            }
        }

        public List<OperationModel> OperationsList { get; set; } = new List<OperationModel>();
        public List<MapElement> MapElements { get; set; } = new List<MapElement>();
        public MapSpan MapRegion { get; set; } = MapSpan.FromCenterAndRadius(new Position(47.640663, -122.1376177), Distance.FromKilometers(5));
    }
}
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Map
{
    public class MapViewModel : BaseViewModel
    {
        private static readonly Position DefaultPosition = new Position(45.767723, 4.835711); // Centers over Lyon

        private readonly OperationsDatabase _operationsDatabase;
        private readonly IPreferences _preferences;

        private readonly MvxSubscriptionToken _token;

        public MapViewModel(OperationsDatabase operationsDatabase, IPreferences preferences, IMvxMessenger messenger)
        {
            _operationsDatabase = operationsDatabase;
            _preferences = preferences;

            _token = messenger.Subscribe<SelectedOpChangedMessage>(async msg => await LoadOperationCommand.ExecuteAsync());
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await LoadOperationCommand.ExecuteAsync();
        }

        #region Properties

        public OperationModel Operation { get; set; }

        public MvxObservableCollection<MapElement> MapElements { get; set; } = new MvxObservableCollection<MapElement>();
        public MvxObservableCollection<Pin> Pins { get; set; } = new MvxObservableCollection<Pin>();
        public MapSpan MapRegion { get; set; } = MapSpan.FromCenterAndRadius(DefaultPosition, Distance.FromKilometers(5));

        #endregion

        #region Commands

        public IMvxAsyncCommand LoadOperationCommand => new MvxAsyncCommand(LoadOperationExecuted);
        private async Task LoadOperationExecuted()
        {
            var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedOpId))
                return;

            MapElements.Clear();
            Pins.Clear();

            Operation = await _operationsDatabase.GetOperationModel(selectedOpId);
            try
            {
                foreach (var link in Operation.Links)
                {
                    var fromPortal = Operation.Portals.First(x => x.Id.Equals(link.FromPortalId));
                    var toPortal = Operation.Portals.First(x => x.Id.Equals(link.ToPortalId));

                    var culture = CultureInfo.GetCultureInfo("en-US");
                    try
                    {
                        double.TryParse(fromPortal.Lat, NumberStyles.Float, culture, out var fromLat);
                        double.TryParse(fromPortal.Lng, NumberStyles.Float, culture, out var fromLng);
                        double.TryParse(toPortal.Lat, NumberStyles.Float, culture, out var toLat);
                        double.TryParse(toPortal.Lng, NumberStyles.Float, culture, out var toLng);

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
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }

            MapRegion = MapSpan.FromCenterAndRadius(Pins.FirstOrDefault()?.Position ?? DefaultPosition, Distance.FromKilometers(5));
        }

        #endregion
    }
}
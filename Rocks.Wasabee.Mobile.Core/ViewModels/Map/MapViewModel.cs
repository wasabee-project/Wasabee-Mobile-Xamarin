using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms.GoogleMaps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Map
{
    public class MapViewModel : BaseViewModel
    {
        private static readonly Position DefaultPosition = new Position(45.767723, 4.835711); // Centers over Lyon

        private readonly OperationsDatabase _operationsDatabase;
        private readonly IPreferences _preferences;
        private readonly IPermissions _permissions;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigationService;

        private readonly MvxSubscriptionToken _token;

        public MapViewModel(OperationsDatabase operationsDatabase, IPreferences preferences,
            IPermissions permissions, IMvxMessenger messenger, IUserDialogs userDialogs, IMvxNavigationService navigationService)
        {
            _operationsDatabase = operationsDatabase;
            _preferences = preferences;
            _permissions = permissions;
            _userDialogs = userDialogs;
            _navigationService = navigationService;

            _token = messenger.Subscribe<SelectedOpChangedMessage>(async msg => await LoadOperationCommand.ExecuteAsync());
        }

        public override async void Prepare()
        {
            base.Prepare();

            var statusLocationAlways = await _permissions.CheckStatusAsync<Permissions.LocationAlways>();
            var statusLocationWhenInUse = await _permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (statusLocationAlways != PermissionStatus.Granted || statusLocationWhenInUse != PermissionStatus.Granted)
            {
                var result = await _permissions.RequestAsync<Permissions.LocationAlways>();
                statusLocationWhenInUse = await _permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (result != PermissionStatus.Granted && statusLocationWhenInUse != PermissionStatus.Granted)
                {
                    _userDialogs.Alert("Geolocation permission is required to show your position !");
                }
                else
                    GeolocationGranted = true;
            }
            else
            {
                GeolocationGranted = true;
            }
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await LoadOperationCommand.ExecuteAsync();
        }

        #region Properties

        public OperationModel Operation { get; set; }

        public MvxObservableCollection<Polyline> Polylines { get; set; } = new MvxObservableCollection<Polyline>();
        public MvxObservableCollection<Pin> Pins { get; set; } = new MvxObservableCollection<Pin>();
        public MapSpan MapRegion { get; set; } = MapSpan.FromCenterAndRadius(DefaultPosition, Distance.FromKilometers(5));

        public bool GeolocationGranted { get; set; }

        #endregion

        #region Commands

        public IMvxAsyncCommand LoadOperationCommand => new MvxAsyncCommand(LoadOperationExecuted);
        private async Task LoadOperationExecuted()
        {
            var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedOpId))
                return;

            Polylines.Clear();
            Pins.Clear();

            Operation = await _operationsDatabase.GetOperationModel(selectedOpId);
            try
            {
                var culture = CultureInfo.GetCultureInfo("en-US");
                foreach (var link in Operation.Links)
                {
                    var fromPortal = Operation.Portals.First(x => x.Id.Equals(link.FromPortalId));
                    var toPortal = Operation.Portals.First(x => x.Id.Equals(link.ToPortalId));

                    try
                    {
                        double.TryParse(fromPortal.Lat, NumberStyles.Float, culture, out var fromLat);
                        double.TryParse(fromPortal.Lng, NumberStyles.Float, culture, out var fromLng);
                        double.TryParse(toPortal.Lat, NumberStyles.Float, culture, out var toLat);
                        double.TryParse(toPortal.Lng, NumberStyles.Float, culture, out var toLng);

                        Polylines.Add(
                            new Polyline()
                            {
                                StrokeColor = WasabeeColorsHelper.GetColorFromWasabeeName(link.Color),
                                StrokeWidth = 2,
                                Positions =
                                {
                                    new Position(fromLat, fromLng),
                                    new Position(toLat, toLng)
                                }
                            });

                        Pins.Add(new Pin() { Position = new Position(fromLat, fromLng), Label = fromPortal.Name, Icon = BitmapDescriptorFactory.FromBundle($"marker_layer_{Operation.Color}") });
                        Pins.Add(new Pin() { Position = new Position(toLat, toLng), Label = toPortal.Name, Icon = BitmapDescriptorFactory.FromBundle($"marker_layer_{Operation.Color}") });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                foreach (var marker in Operation.Markers)
                {
                    try
                    {
                        var portal = Operation.Portals.First(x => x.Id.Equals(marker.PortalId));
                        double.TryParse(portal.Lat, NumberStyles.Float, culture, out var portalLat);
                        double.TryParse(portal.Lng, NumberStyles.Float, culture, out var portalLng);

                        Pins.Add(new Pin()
                        {
                            Position = new Position(portalLat, portalLng),
                            Label = $"{portal.Name}\r\n\"{portal.Comment}\"",
                            Icon = BitmapDescriptorFactory.FromBundle($"{marker.Type}|{marker.State}")
                        });
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
    }

    #endregion
}
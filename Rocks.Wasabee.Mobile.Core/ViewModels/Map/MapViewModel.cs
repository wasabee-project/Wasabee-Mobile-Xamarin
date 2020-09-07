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
        private readonly TeamsDatabase _teamsDatabase;
        private readonly IPreferences _preferences;
        private readonly IPermissions _permissions;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigationService;
        private readonly IUserSettingsService _userSettingsService;

        private readonly MvxSubscriptionToken _token;

        public MapViewModel(OperationsDatabase operationsDatabase, TeamsDatabase teamsDatabase, IPreferences preferences,
            IPermissions permissions, IMvxMessenger messenger, IUserDialogs userDialogs, IMvxNavigationService navigationService,
            IUserSettingsService userSettingsService)
        {
            _operationsDatabase = operationsDatabase;
            _teamsDatabase = teamsDatabase;
            _preferences = preferences;
            _permissions = permissions;
            _userDialogs = userDialogs;
            _navigationService = navigationService;
            _userSettingsService = userSettingsService;

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

        public bool GeolocationGranted { get; set; }
        public Pin SelectedPin
        {
            get => SelectedWasabeePin?.Pin;
            set
            {
                SelectedWasabeePin = value != null ? Pins.FirstOrDefault(x => x.Pin == value) : null;
                RaisePropertyChanged(() => SelectedPin);
            }
        }

        public OperationModel Operation { get; set; }
        public WasabeePin SelectedWasabeePin { get; set; } = null;

        public MvxObservableCollection<Polyline> Polylines { get; set; } = new MvxObservableCollection<Polyline>();
        public MvxObservableCollection<WasabeePin> Pins { get; set; } = new MvxObservableCollection<WasabeePin>();
        public MapSpan MapRegion { get; set; } = MapSpan.FromCenterAndRadius(DefaultPosition, Distance.FromKilometers(5));
        public MapSpan VisibleRegion { get; set; }

        #endregion

        #region Commands

        public IMvxCommand CloseDetailPanelCommand => new MvxCommand(() => SelectedPin = null);

        public IMvxCommand MoveToPortalCommand => new MvxCommand(MoveToPortalExecuted);
        private void MoveToPortalExecuted()
        {
            if (SelectedWasabeePin == null)
                return;

            var region = MapSpan.FromCenterAndRadius(SelectedWasabeePin.Pin.Position, Distance.FromMeters(200));
            if (VisibleRegion == region)
                RaisePropertyChanged(() => VisibleRegion);
            else
                VisibleRegion = region;
        }

        public IMvxAsyncCommand LoadOperationCommand => new MvxAsyncCommand(LoadOperationExecuted);
        private async Task LoadOperationExecuted()
        {
            var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedOpId))
                return;

            Polylines.Clear();
            Pins.Clear();

            Operation = await _operationsDatabase.GetOperationModel(selectedOpId);
            var teamsAgentsLists = (await _teamsDatabase.GetTeams(_userSettingsService.GetLoggedUserGoogleId()))
                ?.SelectMany(t => t.Agents).Select(a => new { a.Id, a.Name }).Distinct().ToList();
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

                        Pins.Add(new WasabeePin(new Pin() { Position = new Position(fromLat, fromLng), Icon = BitmapDescriptorFactory.FromBundle($"marker_layer_{Operation.Color}") })
                        {
                            Portal = fromPortal
                        });
                        Pins.Add(new WasabeePin(new Pin() { Position = new Position(toLat, toLng), Icon = BitmapDescriptorFactory.FromBundle($"marker_layer_{Operation.Color}") })
                        {
                            Portal = toPortal
                        });
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

                        var pin = new WasabeePin(
                            new Pin()
                            {
                                Position = new Position(portalLat, portalLng),
                                Icon = BitmapDescriptorFactory.FromBundle($"{marker.Type}|{marker.State}")
                            })
                        {
                            Portal = portal,
                            Marker = marker
                        };

                        if (!teamsAgentsLists.IsNullOrEmpty())
                            pin.AssignedTo = teamsAgentsLists!.FirstOrDefault(a => a.Id.Equals(marker.AssignedTo))?.Name;

                        Pins.Add(pin);
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

            MapRegion = MapSpan.FromCenterAndRadius(Pins.FirstOrDefault()?.Pin.Position ?? DefaultPosition, Distance.FromKilometers(5));
        }

        #endregion
    }
}
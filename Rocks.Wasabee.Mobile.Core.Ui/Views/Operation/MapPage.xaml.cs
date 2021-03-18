using MvvmCross;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Operation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxTabbedPagePresentation(Position = TabbedPosition.Tab, NoHistory = true, Icon = "map.png")]
    public partial class MapPage : BaseContentPage<MapViewModel>
    {
        private MvxSubscriptionToken _token;

        private bool _hasLoaded = false;
        private bool _isDetailPanelVisible = false;

        private static int zIndexForLinks = 0;
        private static int zIndexForAnchors = 1;
        private static int zIndexForMarkers = 2;
        private static int zIndexForPlayers = 3;

        private List<Pin> _cachedAgentsPins = new List<Pin>();

        public MapPage()
        {
            InitializeComponent();

            Map.UiSettings.ScrollGesturesEnabled = true;
            Map.UiSettings.ZoomControlsEnabled = true;
            Map.UiSettings.ZoomGesturesEnabled = true;
            Map.UiSettings.MyLocationButtonEnabled = true;

            Map.UiSettings.RotateGesturesEnabled = false;
            Map.UiSettings.TiltGesturesEnabled = false;
            Map.UiSettings.IndoorLevelPickerEnabled = false;
            Map.UiSettings.CompassEnabled = false;
            Map.UiSettings.MapToolbarEnabled = false;

            Map.IsIndoorEnabled = false;
            Map.IsTrafficEnabled = false;
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedWasabeePin")
                AnimateDetailPanel();
            else if (e.PropertyName == "VisibleRegion")
                Map.MoveToRegion(ViewModel.VisibleRegion);
            else if (e.PropertyName == "AgentsPins")
                RefreshAgentsLayer();
            else if (e.PropertyName == "IsLayerLinksActivated")
                RefreshLinksLayer();
            else if (e.PropertyName == "IsLayerAnchorsActivated")
                RefreshAnchorsLayer();
            else if (e.PropertyName == "IsLayerMarkersActivated")
                RefreshMarkersLayer();
            else if (e.PropertyName == "IsLayerAgentsActivated")
                RefreshAgentsLayer();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _token = Mvx.IoCProvider.Resolve<IMvxMessenger>().SubscribeOnMainThread<MessageFrom<MapViewModel>>(msg =>
            {
                _hasLoaded = false;
                RefreshMapView(msg.Data != null && msg.Data is bool data && data);
            });

            AnimateDetailPanel();
            RefreshMapTheme();

            RefreshMapView();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _token?.Dispose();
            _token = null;
        }

        private async void AnimateDetailPanel()
        {
            if (ViewModel.SelectedWasabeePin != null)
            {
                if (_isDetailPanelVisible) return;

                _isDetailPanelVisible = true;
                await DetailPanel.TranslateTo(0, 0, 150); // Show
            }
            else
            {
                _isDetailPanelVisible = false;
                await DetailPanel.TranslateTo(0, 180, 150); // Hide
            }
        }

        private void RefreshMapView(bool moveToRegion = true)
        {
            if (_hasLoaded)
                return;

            Map.Polylines.Clear();
            Map.Pins.Clear();

            RefreshLinksLayer();
            RefreshAnchorsLayer();
            RefreshMarkersLayer();
            RefreshAgentsLayer();

            if (moveToRegion)
                Map.MoveToRegion(ViewModel.OperationMapRegion);

            _hasLoaded = true;
        }

        private void RefreshLinksLayer()
        {
            if (!ViewModel.IsLayerLinksActivated)
            {
                Map.Polylines.Clear();
                return;
            }

            foreach (var link in ViewModel.Links)
            {
                if (Map.Polylines.Any(x => x.Equals(link)))
                    continue;

                link.ZIndex = zIndexForLinks;
                Map.Polylines.Add(link);
            }
        }

        private void RefreshAnchorsLayer()
        {
            foreach (var anchor in ViewModel.Anchors)
            {
                if (Map.Pins.Any(x => x.Equals(anchor.Pin)))
                {
                    if (ViewModel.IsLayerAnchorsActivated)
                        continue;

                    var toRemove = Map.Pins.First(x => x.Equals(anchor.Pin));
                    Map.Pins.Remove(toRemove);
                }
                else
                {
                    if (ViewModel.IsLayerAnchorsActivated)
                    {
                        anchor.Pin.ZIndex = zIndexForAnchors;
                        Map.Pins.Add(anchor.Pin);
                    }
                }
            }
        }

        private void RefreshMarkersLayer()
        {
            foreach (var marker in ViewModel.Markers)
            {
                if (Map.Pins.Any(x => x.Equals(marker.Pin)))
                {
                    if (ViewModel.IsLayerMarkersActivated)
                        continue;

                    var toRemove = Map.Pins.First(x => x.Equals(marker.Pin));
                    Map.Pins.Remove(toRemove);
                }
                else
                {
                    if (ViewModel.IsLayerMarkersActivated)
                    {
                        marker.Pin.ZIndex = zIndexForMarkers;
                        Map.Pins.Add(marker.Pin);
                    }
                }
            }
        }

        private void RefreshAgentsLayer()
        {
            if (ViewModel.AgentsPins.IsNullOrEmpty())
            {
                foreach (var pin in _cachedAgentsPins)
                {
                    var hasRemoved = Map.Pins.Remove(pin);
                }

                _cachedAgentsPins.Clear();

                return;
            }

            foreach (var agentPin in ViewModel.AgentsPins)
            {
                while (_cachedAgentsPins.Any(x => x.Label.Contains(agentPin.AgentName)))
                {
                    var toRemove = _cachedAgentsPins.First(x => x.Label.Contains(agentPin.AgentName));
                    _cachedAgentsPins.Remove(toRemove);

                    Map.Pins.Remove(toRemove);
                }

                if (ViewModel.IsLayerAgentsActivated)
                {
                    agentPin.Pin.ZIndex = zIndexForPlayers;
                    _cachedAgentsPins.Add(agentPin.Pin);

                    Map.Pins.Add(agentPin.Pin);
                }
            }
        }

        private void Map_OnMapClicked(object sender, MapClickedEventArgs e)
        {
            ViewModel.CloseDetailPanelCommand.Execute();
            ViewModel.IsLayerChooserVisible = false;
        }

        private void StyleButton_OnClicked(object sender, EventArgs e)
        {
            ViewModel.SwitchThemeCommand.Execute(
                ViewModel.MapTheme switch
                {
                    MapThemeEnum.GoogleLight => MapThemeEnum.Enlightened,
                    MapThemeEnum.Enlightened => MapThemeEnum.IntelDefault,
                    MapThemeEnum.IntelDefault => MapThemeEnum.RedIntel,
                    MapThemeEnum.RedIntel => MapThemeEnum.GoogleLight,
                    _ => MapThemeEnum.GoogleLight
                });
            RefreshMapTheme();
        }

        private void TypeButton_OnClicked(object sender, EventArgs e)
        {
            ViewModel.SwitchMapTypeCommand.Execute(
                ViewModel.MapType switch
                {
                    MapType.Street => MapType.Hybrid,
                    MapType.Hybrid => MapType.Street,
                    _ => MapType.Street
                });
        }

        private void LayerChooserButton_OnClicked(object sender, EventArgs e)
        {
            ViewModel.IsLayerChooserVisible = !ViewModel.IsLayerChooserVisible;
        }

        private void RefreshMapTheme()
        {
            try
            {
                var resourceName = ViewModel.MapTheme switch
                {
                    MapThemeEnum.GoogleLight => "Rocks.Wasabee.Mobile.Core.Ui.GoogleRoads.MapStyle.json",
                    MapThemeEnum.Enlightened => "Rocks.Wasabee.Mobile.Core.Ui.Greenlightened.MapStyle.json",
                    MapThemeEnum.IntelDefault => "Rocks.Wasabee.Mobile.Core.Ui.Intel.MapStyle.json",
                    MapThemeEnum.RedIntel => "Rocks.Wasabee.Mobile.Core.Ui.RedIntel.MapStyle.json",
                    _ => throw new ArgumentOutOfRangeException(ViewModel.MapTheme.ToString())
                };
                var assembly = typeof(MapPage).GetTypeInfo().Assembly;
                var stream = assembly.GetManifestResourceStream(resourceName);

                string styleFile;
                using (var reader = new System.IO.StreamReader(stream))
                {
                    styleFile = reader.ReadToEnd();
                }

                Map.MapStyle = MapStyle.FromJson(styleFile);
            }
            catch (Exception e)
            {
                Mvx.IoCProvider.Resolve<ILoggingService>().Error(e, $"Error Executing MapPage.RefreshMapTheme({ViewModel.MapTheme})");
            }
        }
    }
}
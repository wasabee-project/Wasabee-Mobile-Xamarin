using MvvmCross;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation.MapElements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Operation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxTabbedPagePresentation(Position = TabbedPosition.Tab, NoHistory = true, Icon = "map.png")]
    public partial class MapPage : BaseContentPage<MapViewModel>
    {
        private enum ZIndexFor
        {
            Zones,
            Links,
            Anchors,
            Markers,
            Players
        }

        private MvxSubscriptionToken _token;

        private bool _hasLoaded;
        private bool _isDetailPanelVisible;
        private bool _isAgentListPanelVisible;

        private readonly List<Pin> _cachedAgentsPins = new List<Pin>();

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
            else if (e.PropertyName == "IsAgentListVisible")
                AnimateAgentListPanel();
            else if (e.PropertyName == "IsLayerLinksActivated")
                RefreshLinksLayer();
            else if (e.PropertyName == "IsLayerAnchorsActivated")
                RefreshAnchorsLayer();
            else if (e.PropertyName == "IsLayerMarkersActivated")
                RefreshMarkersLayer();
            else if (e.PropertyName == "IsLayerAgentsActivated")
                RefreshAgentsLayer();
            else if (e.PropertyName == "IsLayerZonesActivated")
                RefreshZonesLayer();
            else if (e.PropertyName == "Links")
                RefreshLinksLayer();
            else if (e.PropertyName == "Anchors")
                RefreshAnchorsLayer();
            else if (e.PropertyName == "Markers")
                RefreshMarkersLayer();
            else if (e.PropertyName == "Agents")
                RefreshAgentsLayer();
            else if (e.PropertyName == "Zones")
                RefreshZonesLayer();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _token ??= Mvx.IoCProvider.Resolve<IMvxMessenger>().SubscribeOnMainThread<MessageFrom<MapViewModel>>(msg =>
            {
                _hasLoaded = false;
                RefreshMapView(msg.Data is bool data && data);
            });

            AnimateDetailPanel();
            AnimateAgentListPanel();
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

        private async void AnimateAgentListPanel()
        {
            if (ViewModel.IsAgentListVisible)
            {
                if (_isAgentListPanelVisible) return;

                _isAgentListPanelVisible = true;
                await AgentListPanel.TranslateTo(0, 0, 150); // Show
            }
            else
            {
                await AgentListPanel.TranslateTo(180, 0, 150); // Hide
                _isAgentListPanelVisible = false;
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
            RefreshZonesLayer();

            if (moveToRegion)
                Map.MoveToRegion(ViewModel.OperationMapRegion);

            _hasLoaded = true;
        }

        private void RefreshLinksLayer()
        {
            if (!ViewModel.IsLayerLinksActivated || ViewModel.Links.IsNullOrEmpty())
            {
                Map.Polylines.Clear();
                return;
            }

            foreach (var polyline in ViewModel.Links.Select(x => x.Polyline))
            {
                if (Map.Polylines.Any(x => x.Equals(polyline)))
                    continue;

                polyline.ZIndex = (int) ZIndexFor.Links;
                Map.Polylines.Add(polyline);
            }
        }

        private void RefreshAnchorsLayer()
        {
            if (ViewModel.Anchors.IsNullOrEmpty())
            {
                var anchors = new List<Pin>(Map.Pins.Where(x => x.ZIndex == (int) ZIndexFor.Anchors));
                foreach (var anchor in anchors)
                    Map.Pins.Remove(anchor);

                return;
            }

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
                        anchor.Pin.ZIndex = (int) ZIndexFor.Anchors;
                        Map.Pins.Add(anchor.Pin);
                    }
                }
            }
        }

        private void RefreshMarkersLayer()
        {
            if (ViewModel.Markers.IsNullOrEmpty())
            {
                var markers = new List<Pin>(Map.Pins.Where(x => x.ZIndex == (int) ZIndexFor.Markers));
                foreach (var marker in markers)
                    Map.Pins.Remove(marker);

                return;
            }

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
                        marker.Pin.ZIndex = (int) ZIndexFor.Markers;
                        Map.Pins.Add(marker.Pin);
                    }
                }
            }
        }

        private void RefreshAgentsLayer()
        {
            if (ViewModel.Agents.IsNullOrEmpty())
            {
                foreach (var pin in _cachedAgentsPins)
                {
                    Map.Pins.Remove(pin);
                }

                _cachedAgentsPins.Clear();

                return;
            }

            foreach (var agentPin in ViewModel.Agents)
            {
                while (_cachedAgentsPins.Any(x => x.Label.Contains(agentPin.AgentName)))
                {
                    var toRemove = _cachedAgentsPins.First(x => x.Label.Contains(agentPin.AgentName));
                    _cachedAgentsPins.Remove(toRemove);

                    Map.Pins.Remove(toRemove);
                }

                if (ViewModel.IsLayerAgentsActivated)
                {
                    agentPin.Pin.ZIndex = (int) ZIndexFor.Players;
                    _cachedAgentsPins.Add(agentPin.Pin);

                    Map.Pins.Add(agentPin.Pin);
                }
            }
        }

        private void RefreshZonesLayer()
        {
            if (ViewModel.Zones.IsNullOrEmpty() || ViewModel.IsLayerZonesActivated is false)
            {
                Map.Polygons.Clear();
                return;
            }

            if (ViewModel.IsLayerZonesActivated)
            {
                foreach (var wZone in ViewModel.Zones)
                {
                    wZone.Polygon.ZIndex = (int) ZIndexFor.Zones;
                    Map.Polygons.Add(wZone.Polygon);
                }
            }
        }

        private void Map_OnMapClicked(object sender, MapClickedEventArgs e)
        {
            ViewModel.CloseDetailPanelCommand.Execute();
            ViewModel.IsLayerChooserVisible = false;
            ViewModel.IsAgentListVisible = false;
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

        private void AgentListButton_OnClicked(object sender, EventArgs e)
        {
            ViewModel.IsAgentListVisible = !ViewModel.IsAgentListVisible;
        }

        private void RefreshMapTheme()
        {
            try
            {
                const string basePath = "Rocks.Wasabee.Mobile.Core.Ui.Resources.MapStyles";
                var resourceName = ViewModel.MapTheme switch
                {
                    MapThemeEnum.GoogleLight => $"{basePath}.GoogleRoads.json",
                    MapThemeEnum.Enlightened => $"{basePath}.Greenlightened.json",
                    MapThemeEnum.IntelDefault => $"{basePath}.Intel.json",
                    MapThemeEnum.RedIntel => $"{basePath}.RedIntel.json",
                    _ => throw new ArgumentOutOfRangeException(ViewModel.MapTheme.ToString())
                };
                var assembly = typeof(MapPage).GetTypeInfo().Assembly;
                var stream = assembly.GetManifestResourceStream(resourceName);

                if (stream is null)
                    return;

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

        private void AgentsListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is WasabeeAgentPin agent)
            {
                ViewModel.SelectedAgentPin = agent;
                ViewModel.MoveToAgentCommand.Execute(agent);
            }
        }

    }
}
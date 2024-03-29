using MvvmCross;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Plugin.Messenger;
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
        private static object _lockObject = new object();

        private enum ZIndexFor
        {
            Zones,
            Links,
            Anchors,
            Markers,
            Agents
        }

        private MvxSubscriptionToken _token;
        
        private bool _isDetailPanelVisible;
        private bool _isAgentListPanelVisible;

        private bool _globalMapRefreshRunning;

        public MapPage()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);

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

        private bool _firstTime = true;
        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            _token ??= Mvx.IoCProvider.Resolve<IMvxMessenger>().SubscribeOnMainThread<MessageFrom<MapViewModel>>(msg =>
            {
                RefreshMapView(msg.Data is true);
            });

            if (_firstTime)
            {
                _firstTime = false;

                AnimateDetailPanel();
                AnimateAgentListPanel();
                RefreshMapTheme();

                RefreshMapView();
            }
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
            lock (_lockObject)
            {
                _globalMapRefreshRunning = true;

                Map.Pins.Clear();
                Map.Polylines.Clear();
                Map.Polygons.Clear();

                RefreshLinksLayer(true);
                RefreshAnchorsLayer(true);
                RefreshMarkersLayer(true);
                RefreshAgentsLayer(true);
                RefreshZonesLayer(true);

                if (moveToRegion)
                    Map.MoveToRegion(ViewModel.OperationMapRegion);
                _globalMapRefreshRunning = false;
            }
        }

        private void RefreshLinksLayer(bool fromGlobalRefresh = false)
        {
            if (_globalMapRefreshRunning && !fromGlobalRefresh)
                return;

            Map.Polylines.Clear();

            if (ViewModel.IsLayerLinksActivated && ViewModel.Links.Any())
            {
                foreach (var polyline in ViewModel.Links.Select(x => x.Polyline))
                {
                    if (Map.Polylines.Any(x => x.Equals(polyline)))
                    {
                        var toRemove = Map.Polylines.First(x => x.Equals(polyline));
                        Map.Polylines.Remove(toRemove);
                    }

                    polyline.ZIndex = (int) ZIndexFor.Links;
                    Map.Polylines.Add(polyline);
                }
            }
        }

        private void RefreshAnchorsLayer(bool fromGlobalRefresh = false)
        {
            if (_globalMapRefreshRunning && !fromGlobalRefresh)
                return;

            var anchors = new List<Pin>(Map.Pins.Where(x => x.ZIndex == (int) ZIndexFor.Anchors));
            foreach (var anchor in anchors)
                Map.Pins.Remove(anchor);

            if (ViewModel.IsLayerAnchorsActivated && ViewModel.Anchors.Any())
            {
                foreach (var anchor in ViewModel.Anchors.Select(x => x.Pin))
                {
                    if (Map.Pins.Any(x => x.Equals(anchor)))
                    {
                        var toRemove = Map.Pins.First(x => x.Equals(anchor));
                        Map.Pins.Remove(toRemove);
                    }
                    
                    anchor.ZIndex = (int) ZIndexFor.Anchors;
                    Map.Pins.Add(anchor);
                }
            }
        }

        private void RefreshMarkersLayer(bool fromGlobalRefresh = false)
        {
            if (_globalMapRefreshRunning && !fromGlobalRefresh)
                return;

            var markers = new List<Pin>(Map.Pins.Where(x => x.ZIndex == (int) ZIndexFor.Markers));
            foreach (var marker in markers)
                Map.Pins.Remove(marker);

            if (ViewModel.IsLayerMarkersActivated && ViewModel.Markers.Any())
            {
                foreach (var marker in ViewModel.Markers.Select(x => x.Pin))
                {
                    if (Map.Pins.Any(x => x.Equals(marker)))
                    {
                        var toRemove = Map.Pins.First(x => x.Equals(marker));
                        Map.Pins.Remove(toRemove);
                    }
                    
                    marker.ZIndex = (int) ZIndexFor.Markers;
                    Map.Pins.Add(marker);
                }
            }
        }

        private void RefreshAgentsLayer(bool fromGlobalRefresh = false)
        {
            if (_globalMapRefreshRunning && !fromGlobalRefresh)
                return;

            var players = new List<Pin>(Map.Pins.Where(x => x.ZIndex == (int) ZIndexFor.Agents));
            foreach (var player in players)
                Map.Pins.Remove(player);

            if (ViewModel.IsLayerAgentsActivated && ViewModel.Agents.Any())
            {
                foreach (var agent in ViewModel.Agents.Select(x => x.Pin))
                {
                    if (Map.Pins.Any(x => x.Label.Equals(agent.Label)))
                    {
                        var toRemove = Map.Pins.First(x => x.Label.Equals(agent.Label));
                        Map.Pins.Remove(toRemove);
                    }
                    
                    agent.ZIndex = (int) ZIndexFor.Agents;
                    Map.Pins.Add(agent);
                }
            }
        }

        private void RefreshZonesLayer(bool fromGlobalRefresh = false)
        {
            if (_globalMapRefreshRunning && !fromGlobalRefresh)
                return;

            Map.Polygons.Clear();

            if (ViewModel.IsLayerZonesActivated && ViewModel.Zones.Any())
            {
                foreach (var polygon in ViewModel.Zones.Select(x => x.Polygon))
                {
                    if (Map.Polygons.Any(x => x.Equals(polygon)))
                    {
                        var toRemove = Map.Polygons.First(x => x.Equals(polygon));
                        Map.Polygons.Remove(toRemove);
                    }

                    polygon.ZIndex = (int) ZIndexFor.Zones;
                    Map.Polygons.Add(polygon);
                }
            }
        }

        private void Map_OnMapClicked(object sender, MapClickedEventArgs e)
        {
            ViewModel.CloseDetailPanelCommand.Execute();
            ViewModel.IsLayerChooserVisible = false;
            ViewModel.IsAgentListVisible = false;
            ViewModel.SelectedPin = null;
        }

        private void Map_OnPinClicked(object sender, PinClickedEventArgs e)
        {
            ViewModel.SelectedPin = e.Pin;
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

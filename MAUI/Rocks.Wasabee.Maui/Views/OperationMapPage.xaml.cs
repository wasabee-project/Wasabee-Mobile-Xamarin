using Maui.GoogleMaps;
using Rocks.Wasabee.Core.ViewModels.Operation;
using System.ComponentModel;

namespace Rocks.Wasabee.Maui.Views
{
    public partial class OperationMapPage : BaseContentPage<OperationMapViewModel>
    {
        private static readonly Lock LockObject = new();

        private enum ZIndexFor
        {
            Zones,
            Links,
            Anchors,
            Markers,
            Agents
        }

        private bool _globalMapRefreshRunning;

        public OperationMapPage(OperationMapViewModel viewModel) : base(viewModel)
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

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private bool _firstTime = true;
        protected override void OnAppearing()
        {
            base.OnAppearing();

            //_token ??= Mvx.IoCProvider.Resolve<IMvxMessenger>().SubscribeOnMainThread<MessageFrom<MapViewModel>>(msg =>
            //{
            //    RefreshMapView(msg.Data is true);
            //});

            if (_firstTime)
            {
                _firstTime = false;

                //AnimateDetailPanel();
                //AnimateAgentListPanel();
                //RefreshMapTheme();

                RefreshMapView();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedWasabeePin")
                return; //AnimateDetailPanel();
            else if (e.PropertyName == "VisibleRegion")
                Map.MoveToRegion(ViewModel.VisibleRegion);
            else if (e.PropertyName == "IsAgentListVisible")
                return; //AnimateAgentListPanel();
            else if (e.PropertyName is "IsLayerLinksActivated" or "Links")
                RefreshLinksLayer();
            else if (e.PropertyName is "IsLayerAnchorsActivated" or "Anchors")
                RefreshAnchorsLayer();
            else if (e.PropertyName is "IsLayerMarkersActivated" or "Markers")
                RefreshMarkersLayer();
            else if (e.PropertyName is "IsLayerAgentsActivated" or "Agents")
                RefreshAgentsLayer();
            else if (e.PropertyName is "IsLayerZonesActivated" or "Zones")
                RefreshZonesLayer();
        }

        private void RefreshMapView(bool moveToRegion = true)
        {
            lock (LockObject)
            {
                _globalMapRefreshRunning = true;

                Map.Pins.Clear();
                Map.Polylines.Clear();

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

                    polyline.ZIndex = (int)ZIndexFor.Links;
                    Map.Polylines.Add(polyline);
                }
            }
        }

        private void RefreshAnchorsLayer(bool fromGlobalRefresh = false)
        {
            if (_globalMapRefreshRunning && !fromGlobalRefresh)
                return;

            var anchors = new List<Pin>(Map.Pins.Where(x => x.ZIndex == (int)ZIndexFor.Anchors));
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

                    anchor.ZIndex = (int)ZIndexFor.Anchors;
                    Map.Pins.Add(anchor);
                }
            }
        }

        private void RefreshMarkersLayer(bool fromGlobalRefresh = false)
        {
            if (_globalMapRefreshRunning && !fromGlobalRefresh)
                return;

            var markers = new List<Pin>(Map.Pins.Where(x => x.ZIndex == (int)ZIndexFor.Markers));
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

                    marker.ZIndex = (int)ZIndexFor.Markers;
                    Map.Pins.Add(marker);
                }
            }
        }

        private void RefreshAgentsLayer(bool fromGlobalRefresh = false)
        {
            if (_globalMapRefreshRunning && !fromGlobalRefresh)
                return;

            var players = new List<Pin>(Map.Pins.Where(x => x.ZIndex == (int)ZIndexFor.Agents));
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

                    agent.ZIndex = (int)ZIndexFor.Agents;
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

                    polygon.ZIndex = (int)ZIndexFor.Zones;
                    Map.Polygons.Add(polygon);
                }
            }
        }
    }
}
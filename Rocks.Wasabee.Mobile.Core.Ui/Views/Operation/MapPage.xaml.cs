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
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Operation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxTabbedPagePresentation(Position = TabbedPosition.Tab, NoHistory = true)]
    public partial class MapPage : BaseContentPage<MapViewModel>
    {
        private readonly MvxSubscriptionToken _token;

        private List<Polyline> _links = new List<Polyline>();
        private List<Pin> _anchorsPins = new List<Pin>();
        private List<Pin> _markersPins = new List<Pin>();
        private List<Pin> _agentPins = new List<Pin>();

        private bool _hasLoaded = false;
        private bool _isDetailPanelVisible = false;

        public MapPage()
        {
            InitializeComponent();
            Title = "Map";

            _token = Mvx.IoCProvider.Resolve<IMvxMessenger>().SubscribeOnMainThread<MessageFrom<MapViewModel>>(msg =>
            {
                _hasLoaded = false;
                RefreshMapView(msg.Data != null && msg.Data is bool data && data);
            });

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
            else if (e.PropertyName == "IsLocationAvailable")
                Map.MyLocationEnabled = ViewModel.IsLocationAvailable;
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

            RefreshMapTheme();
            RefreshMapView();

            AnimateDetailPanel();
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
                        Map.Pins.Add(marker.Pin);
                    }
                }
            }
        }

        private void RefreshAgentsLayer()
        {
            foreach (var agent in ViewModel.AgentsPins)
            {
                if (Map.Pins.Any(x => x.Label.Contains(agent.AgentName)))
                {
                    if (ViewModel.IsLayerAgentsActivated)
                        continue;

                    var toRemove = Map.Pins.First(x => x.Label.Contains(agent.AgentName));
                    Map.Pins.Remove(toRemove);
                }
                else
                {
                    if (ViewModel.IsLayerAgentsActivated)
                    {
                        Map.Pins.Add(agent.Pin);
                    }
                }
            }

            /*foreach (var agentPin in ViewModel.AgentsPins)
            {
                if (Map.Pins.Any(x => x.Label.Contains(agentPin.AgentName)))
                {
                    var toRemove = Map.Pins.First(x => x.Label.Contains(agentPin.AgentName));
                    Map.Pins.Remove(toRemove);
                    _agentPins.Remove(toRemove);
                }

                _agentPins.Add(agentPin.Pin);

                if (ViewModel.IsLayerAgentsActivated)
                    Map.Pins.Add(agentPin.Pin);
            }*/
        }

        private void Map_OnMapClicked(object sender, MapClickedEventArgs e)
        {
            ViewModel.CloseDetailPanelCommand.Execute();
        }

        private void StyleButton_OnClicked(object sender, EventArgs e)
        {
            ViewModel.SwitchThemeCommand.Execute(
                ViewModel.MapTheme switch
                {
                    MapThemeEnum.GoogleLight => MapThemeEnum.Enlightened,
                    MapThemeEnum.Enlightened => MapThemeEnum.IntelDefault,
                    MapThemeEnum.IntelDefault => MapThemeEnum.GoogleLight,
                    _ => MapThemeEnum.GoogleLight
                });
            RefreshMapTheme();
        }

        private void LayerChooserButton_OnClicked(object sender, EventArgs e)
        {
            ViewModel.IsLayerChooserVisible = !ViewModel.IsLayerChooserVisible;
        }

        private void RefreshMapTheme()
        {
            if (ViewModel.MapTheme == MapThemeEnum.GoogleLight)
            {
                Map.MapStyle = MapStyle.FromJson("[]");
            }
            else
            {
                try
                {
                    var resourceName = ViewModel.MapTheme switch
                    {
                        MapThemeEnum.Enlightened => "Rocks.Wasabee.Mobile.Core.Ui.Greenlightened.MapStyle.json",
                        MapThemeEnum.IntelDefault => "Rocks.Wasabee.Mobile.Core.Ui.Intel.MapStyle.json",
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
}
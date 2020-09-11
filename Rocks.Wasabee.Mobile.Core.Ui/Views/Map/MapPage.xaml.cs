using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Map;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Map
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(NoHistory = true)]
    public partial class MapPage : BaseContentPage<MapViewModel>
    {
        private bool _hasLoaded = false;
        private bool _isDetailPanelVisible = false;

        public MapPage()
        {
            InitializeComponent();
            Title = "Operation Map";
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OperationMapRegion")
            {
                _hasLoaded = false;
                RefreshMapView();
            }
            else if (e.PropertyName == "SelectedWasabeePin")
                AnimateDetailPanel();
            else if (e.PropertyName == "IsLocationAvailable")
                Map.MyLocationEnabled = ViewModel.IsLocationAvailable;
            else if (e.PropertyName == "VisibleRegion")
                Map.MoveToRegion(ViewModel.VisibleRegion);
            else if (e.PropertyName == "AgentsPins")
                RefreshAgentsPins();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            RefreshMapView();

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
            Map.MyLocationEnabled = ViewModel.IsLocationAvailable;

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

        private void RefreshMapView()
        {
            if (_hasLoaded)
                return;

            Map.Polylines.Clear();
            Map.Pins.Clear();

            if (ViewModel.Polylines.Any())
            {
                foreach (var polyline in ViewModel.Polylines.Where(mapElement => !Map.Polylines.Contains(mapElement)))
                {
                    Map.Polylines.Add(polyline);
                }
            }

            if (ViewModel.Pins.Any())
            {
                foreach (var wasabeePin in ViewModel.Pins.Where(wp => !Map.Pins.Contains(wp.Pin)))
                {
                    if (string.IsNullOrEmpty(wasabeePin.Pin.Label))
                        wasabeePin.Pin.Label = string.Empty;

                    Map.Pins.Add(wasabeePin.Pin);
                }
            }

            Map.MoveToRegion(ViewModel.OperationMapRegion);

            _hasLoaded = true;
        }

        private void RefreshAgentsPins()
        {
            foreach (var agentPin in ViewModel.AgentsPins)
            {
                if (Map.Pins.Any(x => x.Label.Contains(agentPin.AgentName)))
                {
                    var toRemove = Map.Pins.First(x => x.Label.Contains(agentPin.AgentName));
                    Map.Pins.Remove(toRemove);
                }

                Map.Pins.Add(agentPin.Pin);
            }
        }

        private void SetMapStyle()
        {
            try
            {
                var assembly = typeof(MapPage).GetTypeInfo().Assembly;
                var stream = assembly.GetManifestResourceStream("Rocks.Wasabee.Mobile.Core.Ui.MapStyle.json");

                string styleFile;
                using (var reader = new System.IO.StreamReader(stream))
                {
                    styleFile = reader.ReadToEnd();
                }

                Map.MapStyle = MapStyle.FromJson(styleFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Map_OnMapClicked(object sender, MapClickedEventArgs e)
        {
            ViewModel.CloseDetailPanelCommand.Execute();
        }

        private bool _isDarkMode = false;
        private void StyleButton_OnClicked(object sender, EventArgs e)
        {
            if (!_isDarkMode)
            {

                _isDarkMode = true;
                SetMapStyle();

                return;
            }

            _isDarkMode = false;
            Map.MapStyle = MapStyle.FromJson("[]");
        }
    }
}
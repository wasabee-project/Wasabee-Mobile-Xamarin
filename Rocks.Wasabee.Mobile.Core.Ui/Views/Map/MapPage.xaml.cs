using MvvmCross;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.ViewModels.Map;
using System;
using System.Collections.Generic;
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
        private readonly MvxSubscriptionToken _token;

        private List<Pin> _agentPins = new List<Pin>();
        private bool _hasLoaded = false;
        private bool _isDetailPanelVisible = false;

        public MapPage()
        {
            InitializeComponent();
            Title = "Operation Map";

            _token = Mvx.IoCProvider.Resolve<IMvxMessenger>().Subscribe<MessageFrom<MapViewModel>>((msg) =>
            {
                _hasLoaded = false;
                RefreshMapView();
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

        private void RefreshMapView()
        {
            if (_hasLoaded)
                return;

            Map.BatchBegin();

            Map.Polylines.Clear();
            Map.Pins.Clear();
            foreach (var agentPin in _agentPins)
                Map.Pins.Add(agentPin);

            Map.BatchCommit();


            Map.BatchBegin();

            if (ViewModel.Polylines.Any())
                foreach (var polyline in ViewModel.Polylines.Where(mapElement => !Map.Polylines.Contains(mapElement)))
                    Map.Polylines.Add(polyline);

            if (ViewModel.Pins.Any())
                foreach (var wasabeePin in ViewModel.Pins.Where(wp => !Map.Pins.Contains(wp.Pin)))
                    Map.Pins.Add(wasabeePin.Pin);

            Map.MoveToRegion(ViewModel.OperationMapRegion);

            Map.BatchCommit();

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
                    _agentPins.Remove(toRemove);
                }

                Map.Pins.Add(agentPin.Pin);
                _agentPins.Add(agentPin.Pin);
            }
        }

        private void Map_OnMapClicked(object sender, MapClickedEventArgs e)
        {
            ViewModel.CloseDetailPanelCommand.Execute();
        }

        private void StyleButton_OnClicked(object sender, EventArgs e)
        {
            ViewModel.SwitchThemeCommand.Execute(ViewModel.MapTheme == MapThemeEnum.Light ? MapThemeEnum.Dark : MapThemeEnum.Light);
            RefreshMapTheme();
        }

        private void RefreshMapTheme()
        {
            if (ViewModel.MapTheme == MapThemeEnum.Light)
            {
                Map.MapStyle = MapStyle.FromJson("[]");
            }
            else if (ViewModel.MapTheme == MapThemeEnum.Dark)
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
        }
    }
}
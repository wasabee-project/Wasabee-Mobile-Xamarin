using MvvmCross.Forms.Presenters.Attributes;
using Rocks.Wasabee.Mobile.Core.ViewModels.Map;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Map
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(NoHistory = true)]
    public partial class MapPage : BaseContentPage<MapViewModel>
    {
        private bool _hasLoaded = false;

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
            if (e.PropertyName == "MapRegion")
            {
                _hasLoaded = false;
                RefreshMapView();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SetMapStyle();
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
            Map.MyLocationEnabled = true;
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
                foreach (var pin in ViewModel.Pins.Where(p => !Map.Pins.Contains(p)))
                {
                    Map.Pins.Add(pin);
                }
            }

            Map.MoveToRegion(ViewModel.MapRegion);

            _hasLoaded = true;
        }

        private void SetMapStyle()
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
    }
}
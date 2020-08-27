using MvvmCross.Forms.Presenters.Attributes;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Map
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation(NoHistory = true)]
    public partial class MapPage
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

            RefreshMapView();
        }

        private void RefreshMapView()
        {
            if (_hasLoaded)
                return;

            Map.MapElements.Clear();
            Map.Pins.Clear();

            if (ViewModel.MapElements.Any())
            {
                foreach (var mapElement in ViewModel.MapElements.Where(mapElement => !Map.MapElements.Contains(mapElement)))
                {
                    Map.MapElements.Add(mapElement);
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
    }
}
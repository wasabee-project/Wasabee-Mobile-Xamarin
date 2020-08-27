using MvvmCross.Forms.Presenters.Attributes;
using System.Linq;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Ui.Views.Map
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [MvxMasterDetailPagePresentation()]
    public partial class MapPage
    {
        private bool _hasLoaded = false;

        public MapPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_hasLoaded)
                return;

            foreach (var mapElement in ViewModel.MapElements.Where(mapElement => !Map.MapElements.Contains(mapElement)))
            {
                Map.MapElements.Add(mapElement);
            }

            foreach (var pin in ViewModel.Pins.Where(p => !Map.Pins.Contains(p)))
            {
                Map.Pins.Add(pin);
            }

            Map.MoveToRegion(ViewModel.MapRegion);

            _hasLoaded = true;
        }
    }
}
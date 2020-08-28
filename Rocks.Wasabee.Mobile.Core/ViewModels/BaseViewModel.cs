using MvvmCross.ViewModels;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public partial class BaseViewModel : MvxViewModel, IMvxViewModel
    {
        protected static IGeolocator Geolocator => CrossGeolocator.Current;

        protected BaseViewModel()
        {

        }

        #region Properties

        public string Title { get; set; }
        public bool IsBusy { get; set; }

        #endregion

        #region Services



        #endregion
    }
}
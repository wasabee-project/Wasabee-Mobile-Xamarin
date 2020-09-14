using MvvmCross;
using MvvmCross.ViewModels;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public partial class BaseViewModel : MvxViewModel, IMvxViewModel
    {
        protected static IGeolocator Geolocator => CrossGeolocator.Current;
        protected static ILoggingService LoggingService => Mvx.IoCProvider.Resolve<ILoggingService>();

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
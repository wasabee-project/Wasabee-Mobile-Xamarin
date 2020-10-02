using MvvmCross;
using MvvmCross.ViewModels;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel, IMvxViewModel
    {
        protected static IGeolocator Geolocator => CrossGeolocator.Current;
        protected static ILoggingService LoggingService => Mvx.IoCProvider.Resolve<ILoggingService>();

        protected BaseViewModel()
        {

        }

        #region Properties

        public string Title { get; set; } = string.Empty;
        public bool IsBusy { get; set; } = false;

        #endregion

        #region Services



        #endregion
    }

    public abstract class BaseDialogViewModel : BaseViewModel, IMvxViewModel
    {

    }
}
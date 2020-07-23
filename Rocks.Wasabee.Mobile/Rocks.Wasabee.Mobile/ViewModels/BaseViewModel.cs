using MvvmCross.ViewModels;

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public partial class BaseViewModel : MvxViewModel, IMvxViewModel
    {
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
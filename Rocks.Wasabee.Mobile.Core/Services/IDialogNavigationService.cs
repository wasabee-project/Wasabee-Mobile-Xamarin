using MvvmCross.ViewModels;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Services
{
    public interface IDialogNavigationService
    {
        Task Navigate<TViewModel>() where TViewModel : IMvxViewModel;
        Task Navigate<TViewModel, TParameter>(TParameter param) where TViewModel : IMvxViewModel<TParameter> where TParameter : class;

        Task Close(bool animated = true);
    }
}
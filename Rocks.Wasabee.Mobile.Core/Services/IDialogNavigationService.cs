using MvvmCross.ViewModels;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Services
{
    public interface IDialogNavigationService
    {
        Task Navigate<TViewModel>() where TViewModel : IMvxViewModel;
        Task Navigate<TViewModel, TParameter>(TParameter param) where TViewModel : IMvxViewModel<TParameter> where TParameter : notnull;
        Task<TResult> Navigate<TViewModel, TResult>() where TViewModel : IMvxViewModelResult<TResult> where TResult : notnull;

        Task<bool> Close(bool animated = true);
        Task<bool> Close<TResult>(IMvxViewModelResult<TResult> viewModel, bool animated = true, TResult? closeResult = default) where TResult : notnull;
    }
}
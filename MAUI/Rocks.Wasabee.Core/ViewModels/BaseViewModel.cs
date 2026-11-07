using CommunityToolkit.Mvvm.ComponentModel;

namespace Rocks.Wasabee.Core.ViewModels
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Title { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsBusy { get; set; } = false;
    }
}



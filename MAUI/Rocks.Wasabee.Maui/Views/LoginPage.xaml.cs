using Rocks.Wasabee.Core.Models;
using Rocks.Wasabee.Core.ViewModels;

namespace Rocks.Wasabee.Maui.Views
{
    public partial class LoginPage : BaseContentPage<LoginPageViewModel>
    {
        //private readonly LoginPageViewModel _viewModel;

        //public LoginPage(LoginPageViewModel loginPageViewModel)
        //{
        //    InitializeComponent();

        //    _viewModel = loginPageViewModel;
        //    BindingContext = _viewModel;
        //}

        public LoginPage(LoginPageViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        private void ServerListView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            CollectionView? cv = sender as CollectionView;
            if (cv?.SelectedItem == null)
                return;

            ViewModel.ChooseServerCommand.ExecuteAsync(e.CurrentSelection.First() as ServerItem);

            cv.SelectedItem = null;
        }
    }
}

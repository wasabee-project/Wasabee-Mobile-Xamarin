using Rocks.Wasabee.Core.Models;
using Rocks.Wasabee.Core.ViewModels;

namespace Rocks.Wasabee.Maui.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginPageViewModel _viewModel;

        public LoginPage(LoginPageViewModel loginPageViewModel)
        {
            InitializeComponent();

            _viewModel = loginPageViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            BindingContext = _viewModel;
        }

        private void ServerListView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var cv = sender as CollectionView;
            if (cv?.SelectedItem == null)
                return;

            _viewModel.ChooseServerCommand.ExecuteAsync(e.CurrentSelection.First() as ServerItem);

            cv.SelectedItem = null;
        }
    }
}

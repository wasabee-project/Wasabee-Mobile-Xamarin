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

        private void ServerListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            _viewModel.ChooseServerCommand.ExecuteAsync(e.Item as ServerItem);
        }
    }
}

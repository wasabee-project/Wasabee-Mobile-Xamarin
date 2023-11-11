using Rocks.Wasabee.Maui.Core.ViewModels;

namespace Rocks.Wasabee.App.Views;

public partial class MainPage : ContentPageBase<MainPageViewModel>
{
	public MainPage(MainPageViewModel viewModel) : base(viewModel)
	{
		InitializeComponent();
	}
}
using Rocks.Wasabee.Maui.Core.ViewModels;

namespace Rocks.Wasabee.App.Views;

public partial class SecondPage : ContentPageBase<SecondPageViewModel>
{
	public SecondPage(SecondPageViewModel viewModel) : base(viewModel)
	{
		InitializeComponent();
	}
}
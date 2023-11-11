using Rocks.Wasabee.App.Views;

namespace Rocks.Wasabee.App;

public partial class WasabeeApp : Application
{
	public WasabeeApp()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}


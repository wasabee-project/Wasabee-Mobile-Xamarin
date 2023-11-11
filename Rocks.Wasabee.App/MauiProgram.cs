using Microsoft.Extensions.Logging;
using Rocks.Wasabee.Maui.Core;

namespace Rocks.Wasabee.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<WasabeeApp>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Initialize Core library
		builder = CoreApp.Configure(builder);

		return builder.Build();
	}
}


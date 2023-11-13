using System.Reflection;
using Microsoft.Extensions.Logging;
using Rocks.Wasabee.App.Views;
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

        builder.Services.AddSingleton<INavigator, Navigator>();

		builder
			.RegisterViewModels()
			.RegisterViews();

        return builder.Build();
	}

	public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
	{
		var currentAssembly = Assembly.GetExecutingAssembly();

		// Transient objects lifetime services are created each time they are requested.
		// This lifetime works best for lightweight, stateless services.
		foreach (var type in currentAssembly.DefinedTypes.Where(e => e.IsAbstract is false && e.GetInterface(nameof(IPage)) is not null))
		{
			builder.Services.AddTransient(type.AsType());
		}

		return builder;
    }
}
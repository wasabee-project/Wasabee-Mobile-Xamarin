using System.Reflection;
using Microsoft.Extensions.Logging;
using Rocks.Wasabee.App.Views;
using Rocks.Wasabee.Maui.Core;
using Rocks.Wasabee.Maui.Core.Configuration;

namespace Rocks.Wasabee.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.RegisterCrossConcerns()
            .RegisterViewModels()
            .RegisterServices()
            .RegisterViews()
            .UseMauiApp<WasabeeApp>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    public static MauiAppBuilder RegisterCrossConcerns(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<WasabeeApp>();
        builder.Services.AddSingleton<INavigator, Navigator>();

        return builder;
    }

    public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        // Transient objects lifetime services are created each time they are requested.
        // This lifetime works best for lightweight, stateless services.
        foreach (var type in currentAssembly.DefinedTypes.Where(e => e.IsSubclassOf(typeof(ContentPage))))
        {
            builder.Services.AddTransient(type.AsType());
        }

        return builder;
    }
}
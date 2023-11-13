using System.Reflection;
using Rocks.Wasabee.Maui.Core.ViewModels;

namespace Rocks.Wasabee.Maui.Core;

public static class CoreApp
{
    public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        // Transient objects lifetime services are created each time they are requested.
        // This lifetime works best for lightweight, stateless services.
        foreach (var type in currentAssembly.DefinedTypes.Where(e => e.Name.EndsWith("Service")))
        {
            builder.Services.AddSingleton(type.AsType());
        }

        return builder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        // Transient objects lifetime services are created each time they are requested.
        // This lifetime works best for lightweight, stateless services.
        foreach (var type in currentAssembly.DefinedTypes.Where(e => e.IsAbstract is false && e.IsSubclassOf(typeof(ViewModelBase))))
        {
            builder.Services.AddTransient(type.AsType());
        }

        return builder;
    }
}
using System.Reflection;

namespace Rocks.Wasabee.Maui.Core.Configuration;

public static class ServicesConfiguration
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
}
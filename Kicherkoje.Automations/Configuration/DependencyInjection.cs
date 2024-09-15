using System.Reactive.Concurrency;
using Kicherkoje.Automations.Apps.StateManagers;
using Microsoft.Extensions.DependencyInjection;

namespace Kicherkoje.Automations.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddStateManagers(this IServiceCollection services)
    {
        services.AddSingleton<ISleepStateManager>(sp => new SleepStateManager(sp.GetRequiredService<IHaContext>(),
            sp.GetRequiredService<ILogger<SleepStateManager>>(), sp.GetRequiredService<IScheduler>()));
        return services;
    }
}
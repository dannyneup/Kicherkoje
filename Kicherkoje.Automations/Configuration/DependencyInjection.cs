using Kicherkoje.Automations.Apps.StateManagers;
using Kicherkoje.Automations.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Kicherkoje.Automations.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddStateManagers(this IServiceCollection services) =>
        services.AddSingleton<ISleepStateManager>(sp => new SleepStateManager(sp.GetRequiredService<IHaContext>(),
            sp.GetRequiredService<ILogger<SleepStateManager>>(), sp.GetRequiredService<ISchedulerService>()));

    public static IServiceCollection AddScheduler(this IServiceCollection services) =>
        services
            .AddQuartz(config =>
            {
                config.UsePersistentStore(options =>
                {
                    options.UseSQLite("Data Source=quartz.sqlite");
                    options.UseNewtonsoftJsonSerializer();
                    config.CheckConfiguration = true;
                });
            })
            .AddSingleton<ISchedulerService, SchedulerService>();
}
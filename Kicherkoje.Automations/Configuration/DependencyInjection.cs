using System.Data;
using Kicherkoje.Automations.Apps.StateManagers;
using Kicherkoje.Automations.Apps.StateManagers.Shared;
using Kicherkoje.Automations.Configuration.HostedServices;
using Kicherkoje.Automations.Shared.Scheduler;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;

namespace Kicherkoje.Automations.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddStateManagers(this IServiceCollection services) =>
        services.AddSingleton<IStateManager<HomeState>>(sp => new HomeStateManager(sp.GetRequiredService<IHaContext>(),
            sp.GetRequiredService<ILogger<HomeStateManager>>(), sp.GetRequiredService<ISchedulerService>()));

    public static IServiceCollection AddScheduler(this IServiceCollection services)
    {
        RegisterSQLiteDBProvider();

        return services
            .AddQuartz(config =>
            {
                config.UsePersistentStore(options =>
                {
                    options.UseGenericDatabase<SQLiteDelegate>(
                        provider: "ms-sqlite",
                        configurer: adoOptions =>
                        {
                            adoOptions.ConnectionString = "Data Source=quartz.sqlite";
                        }
                    );
                    options.UseNewtonsoftJsonSerializer();
                });
            })
            .AddSingleton<ISchedulerService, SchedulerService>()
            .AddHostedService<SchedulerServiceHostedService>();
    }

    private static void RegisterSQLiteDBProvider() =>
        DbProvider.RegisterDbMetadata("ms-sqlite", new DbMetadata
        {
            AssemblyName = typeof(SqliteConnection).Assembly.GetName().Name,
            ConnectionType = typeof(SqliteConnection),
            CommandType = typeof(SqliteCommand),
            ParameterType = typeof(SqliteParameter),
            ParameterDbType = typeof(DbType),
            ParameterDbTypePropertyName = "DbType",
            ParameterNamePrefix = "@",
            ExceptionType = typeof(SqliteException),
            BindByName = true
        });
}
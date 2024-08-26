using Kicherkoje.Automations.Apps.General;
using Kicherkoje.Automations.Apps.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Kicherkoje.Automations.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddAppConfigurations(this IServiceCollection services)
    {
        services.AddScoped<IAppConfig<GeneralLightsConfig>>(provider =>
            new AppConfig<GeneralLightsConfig>(new GeneralLightsConfig
                { HaContext = provider.GetRequiredService<IHaContext>() }));

        return services;
    }
}
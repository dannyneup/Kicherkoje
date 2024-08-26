using System.Reflection;
using Kicherkoje.Automations.Apps.General;
using Kicherkoje.Automations.Apps.Shared;
using Kicherkoje.Automations.Configuration;
using Kicherkoje.Automations.Configuration.HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Extensions.Tts;
using NetDaemon.Runtime;

#pragma warning disable CA1812

try
{
    await Host.CreateDefaultBuilder(args)
        .UseNetDaemonDefaultLogging()
        .UseNetDaemonRuntime()
        .UseNetDaemonTextToSpeech()
        .UseNetDaemonMqttEntityManagement()
        .ConfigureServices(
            (_, services) =>
            {
                services
                    .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
                    .AddNetDaemonStateManager()
                    .AddNetDaemonScheduler()
                    .AddHomeAssistantGenerated()
                    .AddAppConfigurations();
            })
        .Build()
        .RunAsync()
        .ConfigureAwait(false);
}
catch (Exception e)
{
    Console.WriteLine($"Failed to start host... {e}");
    throw;
}
using System.Reflection;
using Kicherkoje.Automations.Configuration;
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
                    .AddStateManagers()
                    .AddScheduler();
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
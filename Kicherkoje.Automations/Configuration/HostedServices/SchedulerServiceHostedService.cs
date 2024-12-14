using System.Threading;
using System.Threading.Tasks;
using Kicherkoje.Automations.Shared.Scheduler;
using Microsoft.Extensions.Hosting;

namespace Kicherkoje.Automations.Configuration.HostedServices;

public class SchedulerServiceHostedService(ISchedulerService schedulerService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken) =>
        await schedulerService.InitializeAsync();

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
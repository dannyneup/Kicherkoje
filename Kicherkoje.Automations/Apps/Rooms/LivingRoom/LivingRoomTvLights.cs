using System.Collections.Generic;
using Kicherkoje.Automations.Apps.Shared;
using Kicherkoje.Automations.Shared.Scheduler;
using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Apps.Rooms.LivingRoom;

[NetDaemonApp(Id = "LivingRoomTvLights")]
public class LivingRoomTvLights : AppBase
{
    public LivingRoomTvLights(IHaContext haContext, ILogger<LivingRoomTvLights> logger, ISchedulerService schedulerService) : base(
        haContext, logger, schedulerService)
    {
        OnSyncBoxDeviceLinked_TriggerLightSync();
    }

    private void OnSyncBoxDeviceLinked_TriggerLightSync()
    {
        var appleTvStatusEntity = Entities.Sensor.SyncBoxHdmi1Status;

        var syncModes = new Dictionary<SensorEntity, HuesyncboxSetSyncStateParameters>
        {
            {
                appleTvStatusEntity,
                new HuesyncboxSetSyncStateParameters
                {
                    Sync = true,
                    Mode = "video",
                    Intensity = "moderate",
                    Input = "input1"
                }
            }
        };

        var deviceId = "5780afbe2e84afb8bf2403cae6e16599";

        Services.Huesyncbox.SetSyncState(new ServiceTarget { DeviceIds = [deviceId] });
    }
}
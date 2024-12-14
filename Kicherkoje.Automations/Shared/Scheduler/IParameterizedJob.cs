using Quartz;

namespace Kicherkoje.Automations.Shared.Scheduler;

// ReSharper disable once UnusedTypeParameter
public interface IParameterizedJob<TParameter> : IJob
    where TParameter : class;
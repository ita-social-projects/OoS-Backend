using System;
using System.Collections.Generic;
using Quartz;

namespace OutOfSchool.WebApi.Util
{
    public static class QuartzPool
    {
        private static List<Action<IServiceCollectionQuartzConfigurator>> quartzConfigActions
            = new List<Action<IServiceCollectionQuartzConfigurator>>();

        public static IReadOnlyList<Action<IServiceCollectionQuartzConfigurator>> GetQuartzConfigActions
            => quartzConfigActions.AsReadOnly();

        public static void AddJob<T>(Action<IJobConfigurator> configure = null)
            where T : IJob
        {
            quartzConfigActions.Add(q => q.AddJob<T>(configure));
        }

        public static void AddTrigger(Action<ITriggerConfigurator> configure = null)
        {
            quartzConfigActions.Add(t => t.AddTrigger(configure));
        }

        public static void Done() => quartzConfigActions = new List<Action<IServiceCollectionQuartzConfigurator>>();
    }
}
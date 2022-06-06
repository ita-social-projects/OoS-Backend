using System;
using System.Collections.Generic;
using Quartz;

namespace OutOfSchool.WebApi.Util
{
    /// <summary>
    /// Represents a pool that contains default quartz tasks. All config actions will be configured by default quartz.
    /// </summary>
    public static class QuartzPool
    {
        private static List<Action<IServiceCollectionQuartzConfigurator>> quartzConfigActions
            = new List<Action<IServiceCollectionQuartzConfigurator>>();

        /// <summary>
        /// Gets all quartz config actions.
        /// </summary>
        public static IReadOnlyList<Action<IServiceCollectionQuartzConfigurator>> GetQuartzConfigActions
            => quartzConfigActions.AsReadOnly();

        /// <summary>
        /// Adds a job to the quartz pool.
        /// </summary>
        /// <param name="configure">Configures a given job.</param>
        /// <typeparam name="T">Quartz job.</typeparam>
        public static void AddJob<T>(Action<IJobConfigurator> configure = null)
            where T : IJob
        {
            quartzConfigActions.Add(q => q.AddJob<T>(configure));
        }

        /// <summary>
        /// Adds a trigger to the quartz pool.
        /// </summary>
        /// <param name="configure">Configures a given trigger.</param>
        public static void AddTrigger(Action<ITriggerConfigurator> configure = null)
        {
            quartzConfigActions.Add(t => t.AddTrigger(configure));
        }

        /// <summary>
        /// Clears all quartz config actions in pool. Make sure they've been configured by default quartz before this action.
        /// </summary>
        public static void ClearAll() => quartzConfigActions = new List<Action<IServiceCollectionQuartzConfigurator>>();
    }
}
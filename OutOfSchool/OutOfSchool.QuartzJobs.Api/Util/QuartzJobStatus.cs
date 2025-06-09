namespace OutOfSchool.QuartzJobs.Api.Util;

/// <summary>
/// Represents the status of a Quartz job.
/// </summary>
public enum QuartzJobStatus
{
    Scheduled,  // The job has active triggers and is waiting for execution.
    Paused,  // The job is paused (all triggers associated with the job are paused).
    Running, // The job is currently running (at least one trigger is firing).
    Mixed,   // The job has mixed states (some triggers are active, some are paused).
    Unknown  // The job status is unknown (e.g., no triggers or unable to determine the state).
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OutOfSchool.QuartzJobs.Api.Services;

namespace OutOfSchool.QuartzJobs.Api.Extensions;

public static class QuartzMonitoringEndpoints
{
    /// <summary>
    /// Maps Quartz monitoring API endpoints for jobs information, history, errors, and upcoming executions.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The updated endpoint route builder.</returns>
    public static RouteGroupBuilder MapQuartzMonitoringApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/quartz").WithTags("Quartz Jobs");

        group.MapGet("/jobs", async (IQuartzMonitoringService service) =>
        {
            var jobs = await service.GetAllJobsAsync();
            return Results.Ok(jobs);
        })
        .WithSummary("Get all jobs with their current status.")
        .WithDescription("Returns a list of all jobs with their details and calculated status: Scheduled, Paused, Running, Mixed or Unknown.");


        group.MapGet("/jobs/running", async (IQuartzMonitoringService service) =>
        {
            var jobs = await service.GetRunningJobsAsync();
            return Results.Ok(jobs);
        })
        .WithSummary("Get currently running jobs.")
        .WithDescription("Returns all jobs that are currently being executed, including their start time and trigger information.");


        group.MapGet("/jobs/{jobName}", async (string jobName, IQuartzMonitoringService service) =>
        {
            var job = await service.GetJobDetailsAsync(jobName);
            return job is not null 
                ? Results.Ok(job) 
                : Results.NotFound($"Job with name '{jobName}' was not found.");
        })
        .WithSummary("Get detailed information about a specific job.")
        .WithDescription("Returns job details, trigger keys, and upcoming execution times for a specific job by name.");


        group.MapGet("/jobs/{jobName}/history", async (string jobName, IQuartzMonitoringService service) =>
        {
            var history = await service.GetJobHistoryAsync(jobName);
            return history.Any() 
                ? Results.Ok(history) 
                : Results.NotFound($"Job with name '{jobName}' was not found.");
        })
        .WithSummary("Get execution history of a specific job.")
        .WithDescription("Returns execution history (success and failure logs) for a specific job.");


        group.MapGet("/jobs/{jobName}/errors", async (string jobName, IQuartzMonitoringService service) =>
        {
            var failed = await service.GetFailedJobsAsync(jobName);
            return failed.Any() 
                ? Results.Ok(failed) 
                : Results.NotFound($"Job with name '{jobName}' was not found.");
        })
        .WithSummary("Get failed execution history of a specific job.")
        .WithDescription("Returns only failed execution logs for a specific job.");


        group.MapGet("/jobs/scheduled", async (IQuartzMonitoringService service) =>
        {
            var result = await service.GetNextExecutionsForAllJobsAsync();
            return Results.Ok(result);
        })
        .WithSummary("Get upcoming execution times for all jobs.")
        .WithDescription("Returns the next planned execution times for all jobs and their triggers.");


        group.MapGet("/jobs/{jobName}/scheduled", async (string jobName, IQuartzMonitoringService service) =>
        {
            var result = await service.GetNextExecutionsForJobAsync(jobName);
            return result.Any() 
                ? Results.Ok(result) 
                : Results.NotFound($"Job with name '{jobName}' was not found.");
        })
        .WithSummary("Get upcoming execution times for a specific job.")
        .WithDescription("Returns the next planned execution times for all triggers of a specific job by name.");

        return group;
    }
}

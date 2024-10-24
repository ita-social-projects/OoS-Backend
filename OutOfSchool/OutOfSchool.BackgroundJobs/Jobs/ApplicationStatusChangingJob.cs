using OutOfSchool.BusinessLogic.Services;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;

public class ApplicationStatusChangingJob : IJob
{
    private readonly IApplicationService applicationService;

    public ApplicationStatusChangingJob(IApplicationService applicationService)
    {
        this.applicationService = applicationService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await applicationService.ChangeApprovedStatusesToStudying().ConfigureAwait(false);
    }
}

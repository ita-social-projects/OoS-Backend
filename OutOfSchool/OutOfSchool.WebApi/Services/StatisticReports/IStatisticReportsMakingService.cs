namespace OutOfSchool.WebApi.Services.StatisticReports;

public interface IStatisticReportsMakingService
{
    /// <summary>
    /// Asynchronously creating statistic reports.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task CreateStatisticReports(CancellationToken cancellationToken = default);
}

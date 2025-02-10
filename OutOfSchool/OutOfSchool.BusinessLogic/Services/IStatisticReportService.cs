using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StatisticReports;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for StatisticReport entity.
/// </summary>
public interface IStatisticReportService
{
    /// <summary>
    /// Get all statistic reports from the database.
    /// </summary>
    /// <param name="filter">Filter to get a part of all statistic reports that were found.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The result is a <see cref="SearchResult{StatisticReportDto}"/> that contains the count of all found statistic reports and a list of statistic reports that were received.</returns>
    /// <exception cref="ArgumentNullException">If one of the parameters was null.</exception>
    /// <exception cref="ArgumentException">If one of the offsetFilter's properties is negative.</exception>
    Task<SearchResult<StatisticReportDto>> GetByFilter(StatisticReportFilter filter);

    /// <summary>
    /// Get title by file's key.
    /// </summary>
    /// <param name="externalId">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<string> GetNameByExternalId(string externalId);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="externalId">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<FileModel> GetDataById(string externalId);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="statisticReport">Model for creating entity.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Create(StatisticReport statisticReport);

    /// <summary>
    /// Delete entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(Guid id);
}

﻿using System.Linq.Expressions;
using AutoMapper;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Files;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.StatisticReports;

namespace OutOfSchool.WebApi.Services;

public class StatisticReportService : IStatisticReportService
{
    private readonly IStatisticReportFileStorage storage;
    private readonly IStatisticReportRepository statisticReportRepository;
    private readonly ILogger<StatisticReportService> logger;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticReportService"/> class.
    /// </summary>
    /// <param name="storage">Storage for StatisticReport entity.</param>
    /// <param name="statisticReportRepository">Repository for the StatisticReport entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    public StatisticReportService(
        IStatisticReportFileStorage storage,
        IStatisticReportRepository statisticReportRepository,
        ILogger<StatisticReportService> logger,
        IMapper mapper)
    {
        this.storage = storage;
        this.statisticReportRepository = statisticReportRepository;
        this.logger = logger;
        this.mapper = mapper;
    }

    public async Task Create(StatisticReport statisticReport)
    {
        await statisticReportRepository.Create(statisticReport).ConfigureAwait(false);
    }

    public async Task Delete(Guid id)
    {
        logger.LogInformation("Deleting StatisticReport with Id = {Id} started", id);

        var entity = new StatisticReport { Id = id };

        try
        {
            await statisticReportRepository.Delete(entity).ConfigureAwait(false);

            logger.LogInformation("StatisticReport with Id = {Id} successfully deleted", id);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError("Deleting failed. StatisticReport with Id = {Id} doesn't exist in the system", id);
            throw;
        }
    }

    public async Task<SearchResult<StatisticReportDto>> GetByFilter(StatisticReportFilter filter)
    {
        logger.LogInformation($"Getting StatisticReports by filter");

        filter ??= new StatisticReportFilter();

        var predicate = PredicateBuild(filter);

        var sortExpression = new Dictionary<Expression<Func<StatisticReport, object>>, SortDirection>()
        {
            { sr => sr.Date, SortDirection.Ascending },
        };

        var totalAmount = await statisticReportRepository.Count(where: predicate).ConfigureAwait(false);
        var statisticReports = await statisticReportRepository.Get(
            skip: filter.From,
            take: filter.Size,
            where: predicate,
            includeProperties: "Workshop,Child,Parent",
            orderBy: sortExpression).ToListAsync().ConfigureAwait(false);

        logger.LogInformation(!statisticReports.Any()
            ? $"There is no StatisticReports in the Db with filter."
            : $"Successfully got StatisticReports with filter.");

        var searchResult = new SearchResult<StatisticReportDto>()
        {
            TotalAmount = totalAmount,
            Entities = mapper.Map<List<StatisticReportDto>>(statisticReports),
        };

        return searchResult;
    }

    public async Task<FileModel> GetDataById(string externalId)
    {
        try
        {
            return await storage.GetByIdAsync(externalId).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "File with id='{externalId}' wasn't found", externalId);
            return null;
        }
    }

    private static Expression<Func<StatisticReport, bool>> PredicateBuild(StatisticReportFilter filter)
    {
        var predicate = PredicateBuilder.True<StatisticReport>();

        if (filter.ReportType is not null)
        {
            predicate = predicate.And(sr => sr.ReportType == filter.ReportType);
        }

        if (filter.ReportDataType is not null)
        {
            predicate = predicate.And(sr => sr.ReportDataType == filter.ReportDataType);
        }

        return predicate;
    }
}

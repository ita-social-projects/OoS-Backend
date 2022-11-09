using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore.Storage;
using Nest;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Files;

namespace OutOfSchool.WebApi.Services.StatisticReports;

public class StatisticReportsMakingService : IStatisticReportsMakingService
{
    private readonly OutOfSchoolDbContext db;
    private readonly IStatisticReportFileStorage storage;
    private readonly IStatisticReportRepository statisticReportRepository;
    private readonly ILogger<StatisticReportsMakingService> logger;

    public StatisticReportsMakingService(
        OutOfSchoolDbContext dbContext,
        IStatisticReportFileStorage storage,
        IStatisticReportRepository statisticReportRepository,
        ILogger<StatisticReportsMakingService> logger)
    {
        this.db = dbContext;
        this.storage = storage;
        this.statisticReportRepository = statisticReportRepository;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task CreateStatisticReports(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Creating statistic reports was started");

        var reportData = await statisticReportRepository.GetDataForReport();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };

        using (var stream = new MemoryStream())
        {
            using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, config))
                {
                    await csv.WriteRecordsAsync(reportData, cancellationToken);
                }

            var fileModel = new FileModel()
            {
                ContentType = "application/octet-stream",
                ContentStream = stream,
            };

            var executionStrategy = db.Database.CreateExecutionStrategy();
            await executionStrategy.Execute(WriteStatisticReports).ConfigureAwait(false);

            async Task WriteStatisticReports()
            {
                await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    var existingReports = 
                        await db.StatisticReports.Where(sr => sr.ReportType == StatisticReportTypes.WorkshopsDaily).ToListAsync();

                    foreach (var report in existingReports)
                    {
                        await statisticReportRepository.Delete(report);
                    }

                    var externalId = await storage.UploadAsync(fileModel, cancellationToken);

                    var currentDate = DateTime.UtcNow;

                    var statisticReport = new StatisticReport()
                    {
                        Date = currentDate,
                        ExternalStorageId = externalId,
                        ReportDataType = StatisticReportDataTypes.CSV,
                        ReportType = StatisticReportTypes.WorkshopsDaily,
                        Title = string.Format(StatisticReportTypes.WorkshopsDaily.GetReportTitle(), currentDate.Date),
                    };

                    await statisticReportRepository.Create(statisticReport);

                    if (currentDate.Day == 1 && currentDate.Month == 1)
                    {
                        externalId = await storage.UploadAsync(fileModel, cancellationToken);

                        statisticReport.ExternalStorageId = externalId;
                        statisticReport.ReportType = StatisticReportTypes.WorkshopsYear;
                        statisticReport.Title = string.Format(statisticReport.ReportType.GetReportTitle(), currentDate.Date);

                        await statisticReportRepository.Create(statisticReport);
                    }

                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    logger.LogError(ex, "Statistic report was not saved.");
                    throw;
                }
            }
        }

        logger.LogDebug("Creating statistic reports was finished");
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Files;

namespace OutOfSchool.Services.Repository;

public class StatisticReportRepository : SensitiveEntityRepository<StatisticReport>, IStatisticReportRepository
{
    private readonly OutOfSchoolDbContext db;
    private readonly IFileInDbRepository fileInDbRepository;

    public StatisticReportRepository(
        OutOfSchoolDbContext dbContext,
        IFileInDbRepository fileInDbRepository)
        : base(dbContext)
    {
        db = dbContext;
        this.fileInDbRepository = fileInDbRepository;
    }

    public async Task<List<StatisticReportCSV>> GetDataForReport()
    {
        var result = await db.StatisticReportsCSV
            .FromSqlRaw<StatisticReportCSV>(@"SELECT * FROM statisticreportdata;")
            .ToListAsync<StatisticReportCSV>();

        return result;
    }

    public new async Task Delete(StatisticReport entity)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));

        db.Entry(entity).State = EntityState.Deleted;

        var fileInDb = await fileInDbRepository.GetById(entity.ExternalStorageId).ConfigureAwait(false);

        if (fileInDb != null)
        {
            db.Entry(fileInDb).State = EntityState.Deleted;
        }

        await db.SaveChangesAsync();
    }
}

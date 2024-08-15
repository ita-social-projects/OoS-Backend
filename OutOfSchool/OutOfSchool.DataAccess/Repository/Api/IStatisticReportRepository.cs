using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IStatisticReportRepository : ISensitiveEntityRepository<StatisticReport>
{
    public Task<List<StatisticReportCSV>> GetDataForReport();
}

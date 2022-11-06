using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IStatisticReportRepository : ISensitiveEntityRepository<StatisticReport>
{
    public Task<List<StatisticReportCSV>> GetDataForReport();
}

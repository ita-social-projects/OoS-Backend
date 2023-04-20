using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.StatisticReports;

public class StatisticReportFilter : OffsetFilter
{
    public StatisticReportTypes? ReportType { get; set; }

    public StatisticReportDataTypes? ReportDataType { get; set; }
}

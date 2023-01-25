using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Services.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum StatisticReportDataTypes
{
    CSV,
    XLSX,
    HTML,
}

public static class StatisticReportDataTypesExtensions
{
    public static string GetContentType(this StatisticReportDataTypes value)
    {
        return value switch
        {
            StatisticReportDataTypes.CSV => "text/csv",
            StatisticReportDataTypes.HTML => "text/html",
            StatisticReportDataTypes.XLSX => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => string.Empty
        };
    }

    public static string GetFileFormat(this StatisticReportDataTypes value)
    {
        return value switch
        {
            StatisticReportDataTypes.CSV => ".csv",
            StatisticReportDataTypes.HTML => ".html",
            StatisticReportDataTypes.XLSX => ".xlsx",
            _ => string.Empty
        };
    }
}
using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
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
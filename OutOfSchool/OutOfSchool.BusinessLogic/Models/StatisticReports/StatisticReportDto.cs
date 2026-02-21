using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.StatisticReports;

public class StatisticReportDto
{
    public Guid Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public StatisticReportTypes ReportType { get; set; }

    [Required]
    public StatisticReportDataTypes ReportDataType { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; }

    [Required]
    public string ExternalStorageId { get; set; }
}

public static class StatisticReportDtoExtensions
{
    public static StatisticReportDto ToDto(this StatisticReport model)
        => new()
        {
            Id = model.Id,
            Date = model.Date,
            ReportType = model.ReportType,
            ReportDataType = model.ReportDataType,
            Title = model.Title,
            ExternalStorageId = model.ExternalStorageId,
        };

    public static List<StatisticReportDto> ToDto(this IEnumerable<StatisticReport> list)
        => list.MapToList(ToDto);
}

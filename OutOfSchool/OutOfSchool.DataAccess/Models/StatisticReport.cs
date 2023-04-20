using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class StatisticReport : IKeyedEntity<Guid>
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

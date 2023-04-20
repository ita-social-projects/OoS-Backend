using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopStatusWithTitleDto : WorkshopStatusDto
{
    public string Title { get; set; } = string.Empty;
}

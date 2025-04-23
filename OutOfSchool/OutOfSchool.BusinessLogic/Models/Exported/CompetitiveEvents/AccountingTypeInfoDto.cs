using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.BusinessLogic.Models.Exported.CompetitiveEvents;

public class AccountingTypeInfoDto
{
    public int Id { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string Title { get; set; }
}

public static class AccountingTypeInfoDtoExtensions
{
    public static AccountingTypeInfoDto ToInfoDto(this CompetitiveEventAccountingType model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
        };
}

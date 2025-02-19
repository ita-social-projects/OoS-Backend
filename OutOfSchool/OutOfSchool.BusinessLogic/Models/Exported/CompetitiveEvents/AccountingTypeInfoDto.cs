using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.CompetitiveEvents;

public class AccountingTypeInfoDto
{
    public int Id { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string Title { get; set; }
}

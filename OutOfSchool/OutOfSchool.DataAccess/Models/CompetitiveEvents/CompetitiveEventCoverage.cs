using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.CompetitiveEvents;

public class CompetitiveEventCoverage : IKeyedEntity<int>, ISoftDeleted
{
    public int Id { get; set; }

    public bool IsDeleted { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string Title { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string TitleEn { get; set; }
}

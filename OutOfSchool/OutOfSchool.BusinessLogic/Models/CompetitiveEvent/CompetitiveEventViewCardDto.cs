using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventViewCardDto
{
    public Guid Id { get; set; }

    [DataType(DataType.Text)]
    public string Title { get; set; }
    
    [DataType(DataType.Text)]
    public string ShortTitle { get; set; }
}

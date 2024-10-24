using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Tag;
public class TagCreate
{
    public long Id { get; set; }

    [Required]
    [MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(60)]
    public string NameEn { get; set; } = string.Empty;
}

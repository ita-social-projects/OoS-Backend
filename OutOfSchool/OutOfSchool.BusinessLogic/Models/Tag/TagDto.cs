using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Tag;
public class TagDto
{
    public long Id { get; set; }

    [MaxLength(60)]
    public string Name { get; set; } = string.Empty;
}

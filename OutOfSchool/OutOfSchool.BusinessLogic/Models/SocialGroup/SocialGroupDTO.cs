using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.SocialGroup;

public class SocialGroupDto
{
    public long Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
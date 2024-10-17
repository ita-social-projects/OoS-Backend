using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;

public class FavoriteDto
{
    public long Id { get; set; }

    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    public string UserId { get; set; }
}
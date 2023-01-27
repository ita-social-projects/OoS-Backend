using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.Services.Models;

[Keyless]
public class CodeficatorParent
{
    [Required]
    public long CatottgsId { get; set; }

    [ForeignKey("CatottgsId")]
    public virtual CATOTTG CATOTTG { get; set; }

    [Required]
    public long ParentId { get; set; }

    [ForeignKey("ParentId")]
    public virtual CATOTTG Parent { get; set; }

    [Required]
    public int Level { get; set; }
}

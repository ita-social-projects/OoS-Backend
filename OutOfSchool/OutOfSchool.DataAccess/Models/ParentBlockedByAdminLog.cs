using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class ParentBlockedByAdminLog : IKeyedEntity<long>
{
    public long Id { get; set; }

    [Required]
    public Guid ParentId { get; set; }

    public virtual Parent Parent { get; set; }

    [Required]
    public string UserId { get; set; }

    public virtual User User { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime OperationDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; }

    [Required]
    public bool IsBlocked { get; set; }
}

using System;

namespace OutOfSchool.Services.Models;

public abstract class BusinessEntity<TKey> : IKeyedEntity<TKey>
{
    public string Document { get; set; }

    public string File { get; set; }

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }

    public User DeletedBy { get; set; }

    public DateTime DeleteDate { get; set; }

    public DateTime DateOfCreationInTheSystem { get; set; }

    public bool IsBlocked { get; set; }

    public User ModifiedBy { get; set; }

    public User CreatedBy { get; set; }

    public bool IsSystemProtected { get; set; }

    public TKey Id { get; set; }
}
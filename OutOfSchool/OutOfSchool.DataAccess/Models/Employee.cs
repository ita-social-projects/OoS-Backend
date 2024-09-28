using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models;

public class Employee : IKeyedEntity<(string, Guid)>, ISoftDeleted
{
    public string UserId { get; set; }

    public bool IsDeleted { get; set; }

    public Guid ProviderId { get; set; }

    public virtual Provider Provider { get; set; }

    public BlockingType BlockingType { get; set; }

    public virtual List<Workshop> ManagedWorkshops { get; set; }

    [NotMapped]
    public (string, Guid) Id
    {
        get => (UserId, ProviderId);
        set
        {
            UserId = value.Item1;
            ProviderId = value.Item2;
        }
    }
}
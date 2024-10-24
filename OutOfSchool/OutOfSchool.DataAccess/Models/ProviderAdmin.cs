using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models;

public class ProviderAdmin : IKeyedEntity<(string, Guid)>, ISoftDeleted
{
    public string UserId { get; set; }

    public bool IsDeleted { get; set; }

    public Guid ProviderId { get; set; }

    public virtual Provider Provider { get; set; }

    // we will use it to check
    // if provider admin is able to access related to his provider objects
    // "true" gives access to all related with base provider workshops.
    // "false" executes further inspection into admins-to-workshops relations
    public bool IsDeputy { get; set; }

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
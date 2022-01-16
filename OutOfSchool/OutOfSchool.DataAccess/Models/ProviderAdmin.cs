using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models
{
    public class ProviderAdmin
    {
        public string UserId { get; set; }

        public Guid ProviderId { get; set; }

        public virtual Provider Provider { get; set; }

        // we will use it to check
        // if provider admin is able to access related to his provider objects
        // "true" gives access to all related with base provider workshops.
        // "false" executes further inspection into admins-to-workshops relations
        public bool IsDeputy { get; set; }
        public virtual List<Workshop> ManagedWorkshops { get; set; }
    }
}
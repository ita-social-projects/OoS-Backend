using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class AboutPortal : IKeyedEntity<Guid>
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        public virtual ICollection<AboutPortalItem> AboutPortalItem { get; set; }
    }
}

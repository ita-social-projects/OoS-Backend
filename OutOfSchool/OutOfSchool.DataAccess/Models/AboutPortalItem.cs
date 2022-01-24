using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class AboutPortalItem : IKeyedEntity<Guid>
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string SectionName { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        public Guid AboutPortalId { get; set; }

        public virtual AboutPortal AboutPortal { get; set; }
    }
}

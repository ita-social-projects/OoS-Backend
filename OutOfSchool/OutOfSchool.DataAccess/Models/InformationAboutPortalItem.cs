using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class InformationAboutPortalItem
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string SectionName { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        public Guid InformationAboutPortalId { get; set; }

        public virtual InformationAboutPortal InformationAboutPortal { get; set; }
    }
}

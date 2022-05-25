using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.SubordinationStructure
{
    public class InstitutionFieldDescription : IKeyedEntity<Guid>
    {
        public Guid Id { get; set; }

        [MinLength(1)]
        [MaxLength(100)]
        public string Title { get; set; }

        public int HierarchyLevel { get; set; }

        [Required]
        public Guid InstitutionId { get; set; }

        public virtual Institution Institution { get; set; }
    }
}

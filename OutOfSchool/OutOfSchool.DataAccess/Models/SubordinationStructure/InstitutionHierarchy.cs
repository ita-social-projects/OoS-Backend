using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.SubordinationStructure
{
    public class InstitutionHierarchy : IKeyedEntity<Guid>
    {
        public Guid Id { get; set; }

        [MinLength(1)]
        [MaxLength(100)]
        public string Title { get; set; }

        public int HierarchyLevel { get; set; }

        public Guid? ParentId { get; set; }

        public virtual InstitutionHierarchy Parent { get; set; }

        [Required]
        public Guid InstitutionId { get; set; }

        public virtual Institution Institution { get; set; }

        public virtual ICollection<Direction> Directions { get; set; }
    }
}

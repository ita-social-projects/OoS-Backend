using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.WebApi.Models.SubordinationStructure
{
    public class InstitutionHierarchyDto
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

        public List<DirectionDto> Directions { get; set; }
    }
}

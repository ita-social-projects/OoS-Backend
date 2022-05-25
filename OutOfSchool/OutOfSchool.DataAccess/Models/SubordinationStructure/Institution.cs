using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.SubordinationStructure
{
    public class Institution : IKeyedEntity<Guid>
    {
        public Guid Id { get; set; }

        [MinLength(1)]
        [MaxLength(100)]
        public string Title { get; set; }

        [Range(1, int.MaxValue)]
        public int NumberOfHierarchyLevels { get; set; }
    }
}

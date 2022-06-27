using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models
{
    public class InstitutionAdmin : IKeyedEntity<long>
    {
        public long Id { get; set; }

        public string UserId { get; set; }

        public Guid InstitutionId { get; set; }
        public virtual Institution Institution { get; set; }
    }
}
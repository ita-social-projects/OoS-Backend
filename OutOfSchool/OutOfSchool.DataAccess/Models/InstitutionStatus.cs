using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OutOfSchool.Services.Models
{
    public class InstitutionStatus : IKeyedEntity<long>
    {
        public InstitutionStatus()
        {
            Providers = new List<Provider>();
        }

        public long Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual IReadOnlyCollection<Provider> Providers { get; set; }
    }
}

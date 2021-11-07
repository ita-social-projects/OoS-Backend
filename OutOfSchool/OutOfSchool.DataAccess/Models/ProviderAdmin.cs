using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models
{
    public class ProviderAdmin
    {
        public string UserId { get; set; }

        public virtual User User { get; set; }

        public Guid ProviderId { get; set; }

        public virtual Provider Provider { get; set; }

        public long? CityId { get; set; }

        public virtual City City { get; set; }
    }
}

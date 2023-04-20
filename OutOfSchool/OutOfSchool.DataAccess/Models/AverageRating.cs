using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class AverageRating : IKeyedEntity<long>
{
    public long Id { get; set; }

    public float Rate { get; set; }

    public int RateQuantity { get; set; }

    [Required]
    public Guid EntityId { get; set; }
}

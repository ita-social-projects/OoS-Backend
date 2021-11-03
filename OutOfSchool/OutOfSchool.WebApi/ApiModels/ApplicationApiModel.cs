using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.ApiModels
{
    public class ApplicationApiModel
    {
        [Required(ErrorMessage = "WorkshopId is required")]
        [Range(1, long.MaxValue, ErrorMessage = "Workshop id should be grater than 0")]
        public Guid WorkshopId { get; set; }

        [Required]
        public IEnumerable<ChildDto> Children { get; set; }
    }
}

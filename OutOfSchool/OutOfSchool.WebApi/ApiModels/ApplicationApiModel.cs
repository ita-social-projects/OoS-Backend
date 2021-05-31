using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.ApiModels
{
    public class ApplicationApiModel
    {
        [Required(ErrorMessage = "WorkshopId is required")]
        [Range(1, long.MaxValue, ErrorMessage = "Workshop id should be grater than 0")]
        public long WorkshopId { get; set; }

        [Required]
        public IEnumerable<ChildDTO> Children { get; set; }
    }
}

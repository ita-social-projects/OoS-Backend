using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.ApiModels
{
    public class ApplicationApiModel
    {
        public long WorkshopId { get; set; }

        public IEnumerable<ChildDTO> Children { get; set; }
    }
}

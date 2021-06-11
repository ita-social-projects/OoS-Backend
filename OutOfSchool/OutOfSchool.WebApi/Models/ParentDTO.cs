using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class ParentDTO
    {
        public long Id { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}

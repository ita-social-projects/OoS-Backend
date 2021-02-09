using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class SocialGroupDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ICollection<long> ChildrenIds { get; set; }
    }
}

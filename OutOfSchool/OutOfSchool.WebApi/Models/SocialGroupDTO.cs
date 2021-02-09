
﻿using System.Collections.Generic;


namespace OutOfSchool.WebApi.Models
{
    public class SocialGroupDTO
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<long> ChildrenIds { get; }
    }
}

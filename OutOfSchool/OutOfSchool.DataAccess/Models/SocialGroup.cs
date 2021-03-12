﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Models
{
    public class SocialGroup
    {
        public long Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public virtual IReadOnlyCollection<Child> Children { get; set; }

        public SocialGroup()
        {
            Children = new List<Child>();
        }
    }
}

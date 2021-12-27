﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OutOfSchool.WebApi.Models.Workshop
{
    public class WorkshopUpdateDto : WorkshopDTO
    {
        public List<IFormFile> ImageFiles { get; set; }
    }
}

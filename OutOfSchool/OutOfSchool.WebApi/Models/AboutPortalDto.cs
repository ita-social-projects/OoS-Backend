using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Util.JsonTools;

namespace OutOfSchool.WebApi.Models
{
    public class AboutPortalDto
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        [ModelBinder(BinderType = typeof(JsonModelBinder))]
        public IEnumerable<AboutPortalItemDto> Items { get; set; }
    }
}

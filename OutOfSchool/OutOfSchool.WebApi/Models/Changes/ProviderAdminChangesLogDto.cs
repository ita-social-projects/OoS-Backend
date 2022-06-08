﻿using System;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Changes
{
    public class ProviderAdminChangesLogDto
    {
        public string ProviderAdminId { get; set; }

        public string ProviderAdminFullName { get; set; }

        public string ProviderTitle { get; set; }

        public string WorkshopTitle { get; set; }

        public string WorkshopCity { get; set; }

        public OperationType OperationType { get; set; }

        public DateTime OperationDate { get; set; }

        public ShortUserDto User { get; set; }
    }
}

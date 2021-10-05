﻿using System;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ElasticsearchSyncRecordDto
    {
        public Guid Id { get; set; }

        public ElasticsearchSyncEntity Entity { get; set; }

        public long RecordId { get; set; }

        public DateTime OperationDate { get; set; }

        public ElasticsearchSyncOperation Operation { get; set; }
    }
}

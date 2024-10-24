﻿using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class ElasticsearchSyncRecordDto
{
    public Guid Id { get; set; }

    public ElasticsearchSyncEntity Entity { get; set; }

    public Guid RecordId { get; set; }

    public DateTimeOffset OperationDate { get; set; }

    public ElasticsearchSyncOperation Operation { get; set; }
}
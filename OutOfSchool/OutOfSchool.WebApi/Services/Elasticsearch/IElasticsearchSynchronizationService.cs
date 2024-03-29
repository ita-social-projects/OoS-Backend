﻿using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Services;

public interface IElasticsearchSynchronizationService
{
    Task AddNewRecordToElasticsearchSynchronizationTable(ElasticsearchSyncEntity entity, Guid id, ElasticsearchSyncOperation operation);

    Task Synchronize(CancellationToken cancellationToken);
}
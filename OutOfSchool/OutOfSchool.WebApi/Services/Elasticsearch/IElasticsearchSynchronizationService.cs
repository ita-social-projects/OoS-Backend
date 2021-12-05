using System;
using System.Threading;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IElasticsearchSynchronizationService
    {
        Task<bool> Synchronize();

        Task Create(ElasticsearchSyncRecordDto dto);

        Task AddNewRecordToElasticsearchSynchronizationTable(ElasticsearchSyncEntity entity, Guid id, ElasticsearchSyncOperation operation);

        Task Synchronize(CancellationToken cancellationToken);
    }
}

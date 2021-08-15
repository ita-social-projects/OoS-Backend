using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IElasticsearchSynchronizationService
    {
        Task<bool> Synchronize();

        Task<IEnumerable<ElasticsearchSyncRecordDto>> GetAll();

        Task<bool> Synchronize2();

        Task<ElasticsearchSyncRecordDto> Create(ElasticsearchSyncRecordDto dto);
    }
}

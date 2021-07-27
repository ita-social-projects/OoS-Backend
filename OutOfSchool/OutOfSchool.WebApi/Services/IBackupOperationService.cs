using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IBackupOperationService
    {
        Task<BackupOperationDto> Create(BackupOperationDto dto);
    }
}

using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IBackupTrackerService
    {
        Task<BackupTrackerDto> Create(BackupTrackerDto dto);
    }
}

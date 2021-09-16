using System.IO;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository
{
    public interface IPictureStorage
    {
        Task<Stream> GetPictureByIdAsync(string pictureId);
    }
}

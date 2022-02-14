using System.Threading.Tasks;

using OutOfSchool.Common;

namespace OutOfSchool.WebApi.Services.Communication.ICommunication
{
    public interface ICommunicationService
    {
        Task<ResponseDto> SendRequest(Request request);
    }
}

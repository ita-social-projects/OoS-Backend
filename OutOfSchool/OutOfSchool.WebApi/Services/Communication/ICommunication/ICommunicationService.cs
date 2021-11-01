using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Communication.ICommunication
{
    public interface ICommunicationService
    {
        Task<T> SendRequest<T>(Request request);
    }
}

using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;

namespace OutOfSchool.BusinessLogic.Services;

public interface IApiErrorService
{
    Task<ApiErrorResponse> AdminsCreatingIsBadRequestDataAttend(AdminBaseDto adminBaseDto, string entityName);
}

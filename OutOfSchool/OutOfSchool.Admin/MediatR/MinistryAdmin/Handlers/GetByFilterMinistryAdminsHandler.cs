using MediatR;
using OutOfSchool.Admin.MediatR.MinistryAdmin.Queries;
using OutOfSchool.Admin.Result;
using OutOfSchool.Admin.Services.MinistryAdmin;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Admin.MediatR.MinistryAdmin.Handlers;
public class GetByFilterMinistryAdminsHandler : IRequestHandler<GetByFilterMinistryAdminsQuery, CustomResult<SearchResult<MinistryAdminDto>>>
{
    private readonly ISensitiveMinistryAdminService _ministryAdminService;

    public GetByFilterMinistryAdminsHandler(ISensitiveMinistryAdminService ministryAdminService)
    {
        _ministryAdminService = ministryAdminService;
    }

    public async Task<CustomResult<SearchResult<MinistryAdminDto>>> Handle(GetByFilterMinistryAdminsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        if (filter == null)
        {
            var message = "Filter can not be null.";
            return CustomResult<SearchResult<MinistryAdminDto>>.Failure(CustomError.ValidationError(message));
        }

        var ministryAdmins = await _ministryAdminService.GetByFilter(filter).ConfigureAwait(false);

        return CustomResult<SearchResult<MinistryAdminDto>>.Success(ministryAdmins);
    }
}

using MediatR;
using OutOfSchool.Admin.MediatR.MinistryAdmin.Queries;
using OutOfSchool.Admin.Services.MinistryAdmin;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Admin.MediatR.MinistryAdmin.Handlers;
public class GetByFilterMinistryAdminsHandler : IRequestHandler<GetByFilterMinistryAdminsQuery, SearchResult<MinistryAdminDto>>
{
    private readonly ISensitiveMinistryAdminService _ministryAdminService;

    public GetByFilterMinistryAdminsHandler(ISensitiveMinistryAdminService ministryAdminService)
    {
        _ministryAdminService = ministryAdminService;
    }

    public async Task<SearchResult<MinistryAdminDto>> Handle(GetByFilterMinistryAdminsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        return await _ministryAdminService.GetByFilter(filter).ConfigureAwait(false);
    }
}

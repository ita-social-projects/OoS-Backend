using MediatR;
using OutOfSchool.Admin.MediatR.Applications.Queries;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Applications.Handlers;
public class GetByFilterAllApplicationsHandler(ISensitiveApplicationService applicationService)
    : IRequestHandler<GetByFilterAllApplicationsQuery, SearchResult<ApplicationDto>>
{
    private readonly ISensitiveApplicationService applicationService = applicationService;

    public async Task<SearchResult<ApplicationDto>> Handle(GetByFilterAllApplicationsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        var applications = await applicationService.GetAll(filter).ConfigureAwait(false);

        return applications;
    }
}

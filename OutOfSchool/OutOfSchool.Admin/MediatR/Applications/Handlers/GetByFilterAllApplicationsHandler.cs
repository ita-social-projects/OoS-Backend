using MediatR;
using OutOfSchool.Admin.MediatR.Applications.Queries;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Applications.Handlers;
internal class GetByFilterAllApplicationsHandler : IRequestHandler<GetByFilterAllApplicationsQuery, CustomResult<SearchResult<ApplicationDto>>>
{
    private readonly ISensitiveApplicationService applicationService;

    public GetByFilterAllApplicationsHandler(ISensitiveApplicationService applicationService)
    {
        this.applicationService = applicationService;
    }

    public async Task<CustomResult<SearchResult<ApplicationDto>>> Handle(GetByFilterAllApplicationsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        if (filter == null)
        {
            var message = "Filter can not be null.";
            return CustomResult<SearchResult<ApplicationDto>>.Failure(CustomError.ValidationError(message));
        }

        var applications = await applicationService.GetAll(filter).ConfigureAwait(false);

        return CustomResult<SearchResult<ApplicationDto>>.Success(applications);
    }
}

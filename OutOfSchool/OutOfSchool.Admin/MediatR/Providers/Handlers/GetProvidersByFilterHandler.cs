using MediatR;
using OutOfSchool.Admin.MediatR.Providers.Queries;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Providers.Handlers;
public class GetProvidersByFilterHandler : IRequestHandler<GetProvidersByFilterQuery, SearchResult<ProviderDto>>
{
    private readonly ISensitiveProviderService providerService;

    public GetProvidersByFilterHandler(ISensitiveProviderService providerService)
    {
        this.providerService = providerService;
    }

    public async Task<SearchResult<ProviderDto>> Handle(GetProvidersByFilterQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        return await providerService.GetByFilter(filter).ConfigureAwait(false);
    }
}

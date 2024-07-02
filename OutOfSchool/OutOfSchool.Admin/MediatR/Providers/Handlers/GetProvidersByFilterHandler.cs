using MediatR;
using OutOfSchool.Admin.MediatR.Providers.Queries;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Providers.Handlers;
public class GetProvidersByFilterHandler : IRequestHandler<GetProvidersByFilterQuery, CustomResult<SearchResult<ProviderDto>>>
{
    private readonly ISensitiveProviderService providerService;

    public GetProvidersByFilterHandler(ISensitiveProviderService providerService)
    {
        this.providerService = providerService;
    }

    public async Task<CustomResult<SearchResult<ProviderDto>>> Handle(GetProvidersByFilterQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        if(filter == null)
        {
            var message = "Filter can not be null.";
            return CustomResult<SearchResult<ProviderDto>>.Failure(CustomError.ValidationError(message));
        }

        var providers = await providerService.GetByFilter(filter).ConfigureAwait(false);

        return CustomResult<SearchResult<ProviderDto>>.Success(providers);
    }
}

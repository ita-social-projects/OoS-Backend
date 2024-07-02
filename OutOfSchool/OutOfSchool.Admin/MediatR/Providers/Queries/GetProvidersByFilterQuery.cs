using MediatR;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;

namespace OutOfSchool.Admin.MediatR.Providers.Queries;
public record GetProvidersByFilterQuery(ProviderFilter Filter) 
    : IRequest<CustomResult<SearchResult<ProviderDto>>>;


using MediatR;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;

namespace OutOfSchool.Admin.MediatR.Applications.Queries;
public sealed record GetByFilterAllApplicationsQuery(ApplicationFilter Filter) 
    : IRequest<SearchResult<ApplicationDto>>;
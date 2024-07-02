using MediatR;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;

namespace OutOfSchool.Admin.MediatR.Applications.Queries;
public record GetByFilterAllApplicationsQuery(ApplicationFilter Filter) 
    : IRequest<CustomResult<SearchResult<ApplicationDto>>>;
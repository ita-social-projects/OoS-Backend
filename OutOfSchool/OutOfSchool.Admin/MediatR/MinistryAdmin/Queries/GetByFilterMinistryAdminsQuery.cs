using MediatR;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Admin.MediatR.MinistryAdmin.Queries;
public record GetByFilterMinistryAdminsQuery(MinistryAdminFilter Filter) 
    : IRequest<CustomResult<SearchResult<MinistryAdminDto>>>;
using MediatR;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Admin.MediatR.MinistryAdmin.Queries;
public sealed record GetByFilterMinistryAdminsQuery(MinistryAdminFilter Filter) 
    : IRequest<SearchResult<MinistryAdminDto>>;
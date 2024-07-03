using MediatR;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Admin.MediatR.MinistryAdmin.Queries;
public record GetByFilterMinistryAdminsQuery(MinistryAdminFilter Filter) 
    : IRequest<SearchResult<MinistryAdminDto>>;
using MediatR;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Admin.MediatR.Directions.Commands;
public sealed record DeleteDirectionByIdCommand(long Id) 
    : IRequest<Result<DirectionDto>>;
using MediatR;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Admin.MediatR.Directions.Commands;
public record UpdateDirectionCommand(DirectionDto DirectionDto) 
    : IRequest<CustomResult<DirectionDto>>;
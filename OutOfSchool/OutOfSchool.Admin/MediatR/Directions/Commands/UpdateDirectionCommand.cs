using MediatR;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Admin.MediatR.Directions.Commands;
public sealed record UpdateDirectionCommand(DirectionDto DirectionDto) 
    : IRequest<DirectionDto>;
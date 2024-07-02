using MediatR;
using OutOfSchool.Admin.MediatR.Directions.Commands;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Directions.Handlers;
public class DeleteDirectionByIdHandler : IRequestHandler<DeleteDirectionByIdCommand, Result<DirectionDto>>
{
    private readonly ISensitiveDirectionService directionService;

    public DeleteDirectionByIdHandler(ISensitiveDirectionService directionService)
    {
        this.directionService = directionService;
    }

    public async Task<Result<DirectionDto>> Handle(DeleteDirectionByIdCommand request, CancellationToken cancellationToken)
    {
        var id = request.Id;

        return await directionService.Delete(id).ConfigureAwait(false);
    }
}

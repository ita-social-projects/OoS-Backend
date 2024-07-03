using MediatR;
using OutOfSchool.Admin.MediatR.Directions.Commands;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Directions.Handlers;
public class UpdateDirectionHandler : IRequestHandler<UpdateDirectionCommand, DirectionDto>
{
    private readonly ISensitiveDirectionService directionService;

    public UpdateDirectionHandler(ISensitiveDirectionService directionService)
    {
        this.directionService = directionService;
    }

    public async Task<DirectionDto> Handle(UpdateDirectionCommand request, CancellationToken cancellationToken)
    {
        var updatedDirectionFromRequest = request.DirectionDto;

        return await directionService.Update(updatedDirectionFromRequest).ConfigureAwait(false);
    }
}

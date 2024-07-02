using MediatR;
using OutOfSchool.Admin.MediatR.Directions.Commands;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Directions.Handlers;
public class UpdateDirectionHandler : IRequestHandler<UpdateDirectionCommand, CustomResult<DirectionDto>>
{
    private readonly ISensitiveDirectionService directionService;

    public UpdateDirectionHandler(ISensitiveDirectionService directionService)
    {
        this.directionService = directionService;
    }

    public async Task<CustomResult<DirectionDto>> Handle(UpdateDirectionCommand request, CancellationToken cancellationToken)
    {
        var updatedDirectionFromRequest = request.DirectionDto;
        
        if(updatedDirectionFromRequest == null)
        {
            var message = "Direction can not be null.";
            return CustomResult<DirectionDto>.Failure(CustomError.ValidationError(message));
        }

        var updatedDirectionFromDatabase = await directionService.Update(updatedDirectionFromRequest).ConfigureAwait(false);

        return CustomResult<DirectionDto>.Success(updatedDirectionFromDatabase);
    }
}

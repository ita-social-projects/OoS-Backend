using MediatR;
using OutOfSchool.Admin.MediatR.Providers.Commands;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common;

namespace OutOfSchool.Admin.MediatR.Providers.Handlers;
public class BlockProviderHandler : IRequestHandler<BlockProviderCommand, CustomResult<ResponseDto>>
{
    private readonly ISensitiveProviderService providerService;

    public BlockProviderHandler(ISensitiveProviderService providerService)
    {
        this.providerService = providerService;
    }

    public async Task<CustomResult<ResponseDto>> Handle(BlockProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = request.ProviderBlockDto;
        var token = request.Token;

        if (provider == null)
        {
            var message = "Provider can not be null.";
            return CustomResult<ResponseDto>.Failure(CustomError.ValidationError(message));
        }

        var response = await providerService.Block(provider, token);

        return CustomResult<ResponseDto>.Success(response);
    }
}

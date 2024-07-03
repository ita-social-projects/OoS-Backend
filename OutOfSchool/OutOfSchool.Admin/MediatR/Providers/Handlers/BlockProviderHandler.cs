using MediatR;
using OutOfSchool.Admin.MediatR.Providers.Commands;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common;

namespace OutOfSchool.Admin.MediatR.Providers.Handlers;
public class BlockProviderHandler(ISensitiveProviderService providerService) : IRequestHandler<BlockProviderCommand, ResponseDto>
{
    private readonly ISensitiveProviderService providerService = providerService;

    public async Task<ResponseDto> Handle(BlockProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = request.ProviderBlockDto;
        var token = request.Token;

        var response = await providerService.Block(provider, token);

        return response;
    }
}

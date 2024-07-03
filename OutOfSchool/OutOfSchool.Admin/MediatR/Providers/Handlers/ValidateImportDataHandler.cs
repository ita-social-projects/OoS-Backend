using MediatR;
using OutOfSchool.Admin.MediatR.Providers.Commands;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Providers.Handlers;
public class ValidateImportDataHandler(ISensitiveProviderService providerService) : IRequestHandler<ValidateImportDataCommand, ImportDataValidateResponse>
{
    private readonly ISensitiveProviderService providerService = providerService;

    public async Task<ImportDataValidateResponse> Handle(ValidateImportDataCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;

        return await providerService.ValidateImportData(data).ConfigureAwait(false);
    }
}

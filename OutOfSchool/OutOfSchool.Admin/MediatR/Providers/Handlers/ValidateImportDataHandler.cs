using MediatR;
using OutOfSchool.Admin.MediatR.Providers.Commands;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Providers.Handlers;
public class ValidateImportDataHandler : IRequestHandler<ValidateImportDataCommand, CustomResult<ImportDataValidateResponse>>
{
    private readonly ISensitiveProviderService providerService;

    public ValidateImportDataHandler(ISensitiveProviderService providerService)
    {
        this.providerService = providerService;
    }

    public async Task<CustomResult<ImportDataValidateResponse>> Handle(ValidateImportDataCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;

        if (data == null)
        {
            var message = "Data can not be null.";
            return CustomResult<ImportDataValidateResponse>.Failure(CustomError.ValidationError(message));
        }

        var result = await providerService.ValidateImportData(data).ConfigureAwait(false);

        return CustomResult<ImportDataValidateResponse>.Success(result);
    }
}

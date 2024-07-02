using MediatR;
using OutOfSchool.Admin.MediatR.Providers.Queries;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Providers.Handlers;
public class ExportProvidersHandler : IRequestHandler<ExportProvidersQuery, CustomResult<byte[]>>
{
    private readonly ISensitiveProviderService providerService;

    public ExportProvidersHandler(ISensitiveProviderService providerService)
    {
        this.providerService = providerService;
    }

    public async Task<CustomResult<byte[]>> Handle(ExportProvidersQuery request, CancellationToken cancellationToken)
    {
        var providersCsvData = await providerService.GetCsvExportData().ConfigureAwait(false);

        if (providersCsvData == null || providersCsvData.Length == 0)
        {
            return CustomResult<byte[]>.Failure(CustomError.None);
        }

        return CustomResult<byte[]>.Success(providersCsvData);
    }
}

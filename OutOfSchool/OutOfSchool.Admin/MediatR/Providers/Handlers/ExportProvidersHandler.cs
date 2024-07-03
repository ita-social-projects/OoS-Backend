using MediatR;
using OutOfSchool.Admin.MediatR.Providers.Queries;
using OutOfSchool.BusinessLogic.Services;

namespace OutOfSchool.Admin.MediatR.Providers.Handlers;
public class ExportProvidersHandler : IRequestHandler<ExportProvidersQuery, byte[]>
{
    private readonly ISensitiveProviderService providerService;

    public ExportProvidersHandler(ISensitiveProviderService providerService)
    {
        this.providerService = providerService;
    }

    public async Task<byte[]> Handle(ExportProvidersQuery request, CancellationToken cancellationToken)
    {
        var providersCsvData = await providerService.GetCsvExportData().ConfigureAwait(false);

        return providersCsvData;
    }
}

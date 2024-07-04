using MediatR;
using OutOfSchool.BusinessLogic.Models.Providers;

namespace OutOfSchool.Admin.MediatR.Providers.Commands;
public sealed record ValidateImportDataCommand(ImportDataValidateRequest Data) 
    : IRequest<ImportDataValidateResponse>;


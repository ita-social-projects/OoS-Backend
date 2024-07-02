using MediatR;
using OutOfSchool.Admin.Result;
using OutOfSchool.BusinessLogic.Models.Providers;

namespace OutOfSchool.Admin.MediatR.Providers.Commands;
public record ValidateImportDataCommand(ImportDataValidateRequest Data) 
    : IRequest<CustomResult<ImportDataValidateResponse>>;


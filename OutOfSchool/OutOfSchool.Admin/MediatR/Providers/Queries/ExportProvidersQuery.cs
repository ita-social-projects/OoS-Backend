using MediatR;
using OutOfSchool.Admin.Result;

namespace OutOfSchool.Admin.MediatR.Providers.Queries;
public record ExportProvidersQuery() : IRequest<CustomResult<byte[]>>;

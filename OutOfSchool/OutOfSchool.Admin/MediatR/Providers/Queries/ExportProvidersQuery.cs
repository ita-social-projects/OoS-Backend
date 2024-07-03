using MediatR;

namespace OutOfSchool.Admin.MediatR.Providers.Queries;
public record ExportProvidersQuery() : IRequest<byte[]>;

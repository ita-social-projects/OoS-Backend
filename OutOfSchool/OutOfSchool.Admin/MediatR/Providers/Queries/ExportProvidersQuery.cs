using MediatR;

namespace OutOfSchool.Admin.MediatR.Providers.Queries;
public sealed record ExportProvidersQuery() : IRequest<byte[]>;

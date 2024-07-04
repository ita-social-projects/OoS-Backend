using MediatR;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.Common;

namespace OutOfSchool.Admin.MediatR.Providers.Commands;
public sealed record BlockProviderCommand(ProviderBlockDto ProviderBlockDto, string? Token = null)
    : IRequest<ResponseDto>;
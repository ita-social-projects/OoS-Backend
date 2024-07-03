﻿using MediatR;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.Common;

namespace OutOfSchool.Admin.MediatR.Providers.Commands;
public record BlockProviderCommand(ProviderBlockDto ProviderBlockDto, string? Token = null)
    : IRequest<ResponseDto>;
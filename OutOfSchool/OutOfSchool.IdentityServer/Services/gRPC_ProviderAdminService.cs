using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.IdentityServer.Services.Intefaces;

namespace GrpcServiceServer
{
    [Authorize(AuthenticationSchemes = Constants.BearerScheme)]
    public class GRPC_ProviderAdminService : gRPC_ProviderAdmin.gRPC_ProviderAdminBase
    {
        private readonly ILogger<GRPC_ProviderAdminService> logger;
        private readonly IProviderAdminService providerAdminService;
        private readonly IMapper mapper;

        public GRPC_ProviderAdminService(
            ILogger<GRPC_ProviderAdminService> logger,
            IProviderAdminService providerAdminService,
            IMapper mapper)
        {
            this.logger = logger;
            this.providerAdminService = providerAdminService;
            this.mapper = mapper;
        }

        public override async Task<CreateReply> CreateProviderAdmin(CreateRequest request, ServerCallContext context)
        {
            var createProviderAdminDto = mapper.Map<CreateProviderAdminDto>(request);

            var userId = context.GetHttpContext().User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            var result = await providerAdminService.CreateProviderAdminAsync(createProviderAdminDto, null, userId, "");
            var resultCreateProviderAdminDto = (CreateProviderAdminDto)result.Result;

            return mapper.Map<CreateReply>(resultCreateProviderAdminDto);
        }
    }
}

using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using GrpcService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;
using OutOfSchool.IdentityServer.Services.Intefaces;

namespace GrpcServiceServer
{
    [Authorize(AuthenticationSchemes = Constants.BearerScheme)]
    public class ProviderAdminServiceGRPC : GRPCProviderAdmin.GRPCProviderAdminBase
    {
        private readonly ILogger<ProviderAdminServiceGRPC> logger;
        private readonly IProviderAdminService providerAdminService;
        private readonly IMapper mapper;

        public ProviderAdminServiceGRPC(
            ILogger<ProviderAdminServiceGRPC> logger,
            IProviderAdminService providerAdminService,
            IMapper mapper)
        {
            this.logger = logger;
            this.providerAdminService = providerAdminService;
            this.mapper = mapper;
        }

        public override async Task<CreateProviderAdminReply> CreateProviderAdmin(CreateProviderAdminRequest request, ServerCallContext context)
        {
            var createProviderAdminDto = mapper.Map<CreateProviderAdminDto>(request);

            var userId = context.GetHttpContext().User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
            var result = await providerAdminService.CreateProviderAdminAsync(createProviderAdminDto, null, userId, request.RequestId);
            CreateProviderAdminReply createProviderAdminReply;

            if (result.IsSuccess && result.Result is CreateProviderAdminDto resultCreateProviderAdminDto)
            {
                createProviderAdminReply = mapper.Map<CreateProviderAdminReply>(resultCreateProviderAdminDto);
                createProviderAdminReply.IsSuccess = true;
            }
            else
            {
                createProviderAdminReply = new CreateProviderAdminReply() { IsSuccess = false };
            }

            return createProviderAdminReply;
        }
    }
}
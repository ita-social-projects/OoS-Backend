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
            //var providerAdminDto = mapper.Map<CreateProviderAdminDto>(request);

            CreateProviderAdminDto providerAdminDto = new CreateProviderAdminDto();
            providerAdminDto.FirstName = request.FirstName;
            providerAdminDto.LastName = request.LastName;
            providerAdminDto.MiddleName = request.MiddleName;
            providerAdminDto.Email = request.Email;
            providerAdminDto.PhoneNumber = request.PhoneNumber;
            providerAdminDto.CreatingTime = request.CreatingTime.ToDateTimeOffset();
            providerAdminDto.ReturnUrl = request.ReturnUrl;
            providerAdminDto.ProviderId = Guid.Parse(request.ProviderId);
            providerAdminDto.UserId = request.UserId;
            providerAdminDto.IsDeputy = request.IsDeputy;
            providerAdminDto.ManagedWorkshopIds = new List<Guid>();

            foreach (string item in request.ManagedWorkshopIds)
            {
                providerAdminDto.ManagedWorkshopIds.Add(Guid.Parse(item));
            }

            //var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
            var userId = context.GetHttpContext().User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            var result = await providerAdminService.CreateProviderAdminAsync(providerAdminDto, null, userId, "");
            var _result = (CreateProviderAdminDto)result.Result;

            var resultGRPC = new CreateReply();

            if (!(_result is null))
            {
                resultGRPC.FirstName = _result.FirstName;
                resultGRPC.LastName = _result.LastName;
                resultGRPC.MiddleName = _result.MiddleName;
                resultGRPC.Email = _result.Email;
                resultGRPC.PhoneNumber = _result.PhoneNumber;
                resultGRPC.CreatingTime = Timestamp.FromDateTimeOffset(_result.CreatingTime);
                resultGRPC.ReturnUrl = _result.ReturnUrl;
                resultGRPC.ProviderId = _result.ProviderId.ToString();
                resultGRPC.UserId = _result.UserId;
                resultGRPC.IsDeputy = _result.IsDeputy;

                foreach (Guid item in _result.ManagedWorkshopIds)
                {
                    resultGRPC.ManagedWorkshopIds.Add(item.ToString());
                }
            }

            return resultGRPC;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using GrpcService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.Models;
using OutOfSchool.IdentityServer.Services.Intefaces;

namespace GrpcServiceServer
{
    public class gRPC_ProviderAdminService : gRPC_ProviderAdmin.gRPC_ProviderAdminBase
    {
        private readonly ILogger<gRPC_ProviderAdminService> logger;
        private readonly IProviderAdminService providerAdminService;
        private readonly IMapper mapper;
        //private readonly IUrlHelper urlHelper;

        public gRPC_ProviderAdminService(
            ILogger<gRPC_ProviderAdminService> logger,
            IProviderAdminService providerAdminService,
            IMapper mapper)
        {
            this.logger = logger;
            this.providerAdminService = providerAdminService;
            this.mapper = mapper;
            //this.urlHelper = urlHelper;
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

            var result = await providerAdminService.CreateProviderAdminAsync(providerAdminDto, null, "", "");

            return await Task.FromResult(new CreateReply
            {
                Message = "Hello " + request.FirstName
            });
        }
    }
}

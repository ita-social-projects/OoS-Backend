using AutoMapper;
using Grpc.Core;
using GrpcService;
using OutOfSchool.Common.Models;

namespace GrpcServiceServer;

[Authorize]
public class ProviderAdminServiceGrpc : GRPCProviderAdmin.GRPCProviderAdminBase
{
    private readonly IProviderAdminService providerAdminService;
    private readonly IMapper mapper;

    public ProviderAdminServiceGrpc(
        IProviderAdminService providerAdminService,
        IMapper mapper)
    {
        this.providerAdminService = providerAdminService;
        this.mapper = mapper;
    }

    public override async Task<CreateProviderAdminReply> CreateProviderAdmin(CreateProviderAdminRequest request, ServerCallContext context)
    {
        var createProviderAdminDto = mapper.Map<CreateEmployeeDto>(request);

        var userId = context.GetHttpContext().User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
        var result = await providerAdminService.CreateEmployeeAsync(createProviderAdminDto, null, userId);
        CreateProviderAdminReply createProviderAdminReply;

        if (result.IsSuccess && result.Result is CreateEmployeeDto resultCreateProviderAdminDto)
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
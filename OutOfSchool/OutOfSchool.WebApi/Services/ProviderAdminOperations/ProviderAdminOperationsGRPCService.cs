using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using GrpcService;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Services.GRPC;

namespace OutOfSchool.WebApi.Services.ProviderAdminOperations;

/// <summary>
/// Implements the interface for creating ProviderAdmin.
/// </summary>
public class ProviderAdminOperationsGRPCService : IProviderAdminOperationsService
{
    private readonly ILogger<ProviderAdminService> logger;
    private readonly IMapper mapper;
    private readonly IGRPCCommonService gRPCCommonService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderAdminOperationsGRPCService"/> class.
    /// </summary>
    /// <param name="mapper">Mapper.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="gRPCCommonService">GRPCCommonService.</param>
    public ProviderAdminOperationsGRPCService(
        IMapper mapper,
        ILogger<ProviderAdminService> logger,
        IGRPCCommonService gRPCCommonService)
    {
        this.mapper = mapper;
        this.logger = logger;
        this.gRPCCommonService = gRPCCommonService;
    }

    /// <inheritdoc/>
    public async Task<ResponseDto> CreateProviderAdminAsync(string userId, CreateProviderAdminDto providerAdminDto, string token)
    {
        using var channel = gRPCCommonService.CreateAuthenticatedChannel(token);
        var client = new GRPCProviderAdmin.GRPCProviderAdminClient(channel);

        var createProviderAdminRequest = mapper.Map<CreateProviderAdminRequest>(providerAdminDto);

        var response = new ResponseDto()
        {
            IsSuccess = true,
            HttpStatusCode = HttpStatusCode.OK,
        };

        string requestId = Guid.NewGuid().ToString();
        createProviderAdminRequest.RequestId = requestId;

        logger.LogDebug($"GRPC: Request(id): {requestId} was sent. " +
                        $"User(id): {userId}. Method: CreateProviderAdminAsync.");

        try
        {
            var reply = await client.CreateProviderAdminAsync(createProviderAdminRequest);

            if (reply.IsSuccess)
            {
                var providerAdminDtoGRPC = mapper.Map<CreateProviderAdminDto>(reply);

                response.IsSuccess = true;
                response.Result = providerAdminDtoGRPC;

                return response;
            }
            else
            {
                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
            }
        }
        catch (RpcException ex)
        {
            logger.LogError($"GRPC: Request(id): {requestId}. " +
                            $"Admin was not created by User(id): {userId}. {ex.Message}");

            response.IsSuccess = false;
            response.HttpStatusCode = HttpStatusCode.InternalServerError;
            response.Message = ex.Message;
        }

        return response;
    }
}
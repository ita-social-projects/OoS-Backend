using System.Text.Json;
using Microsoft.Extensions.Options;
using OutOfSchool.AuthCommon;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Models;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Models.ExternalAuth;

namespace OutOfSchool.AuthorizationServer.Services;

public class GovIdentityCommunicationService : CommunicationService, IGovIdentityCommunicationService
{
    private readonly AuthorizationServerConfig authServerConfig;

    public GovIdentityCommunicationService(
        IHttpClientFactory httpClientFactory,
        IOptions<CommunicationConfig> communicationConfig,
        ILogger<GovIdentityCommunicationService> logger,
        IOptions<AuthorizationServerConfig> authServerConfig)
        : base(httpClientFactory, communicationConfig, logger)
    {
        this.authServerConfig = authServerConfig.Value;
    }

    public Task<Either<IErrorResponse, UserInfoResponse>> GetUserInfo(string remoteUserId, string remoteToken) =>
        SendRequest<CertificateResponse, IErrorResponse>(
                CreateCertRequest(),
                new EncryptionErrorHandler("The certificate could not be retrieved."))
            .FlatMapAsync(
                certResponse => SendRequest<EnvelopedUserInfoResponse, IErrorResponse>(
                    CreateUserInfoRequest(remoteUserId, certResponse.CertBase64, remoteToken),
                    new IdGovUaErrorHandler()))
            .FlatMapAsync(
                encryptedUser => SendRequest<UserInfoResponse, IErrorResponse>(
                    CreateDecryptionRequest(encryptedUser),
                    new EncryptionErrorHandler("User info could not be decrypted.")));

    private Request CreateCertRequest()
    {
        return new Request
        {
            HttpMethodType = HttpMethodType.Get,
            Url = new Uri(
                authServerConfig.ExternalLogin.EUSignServiceUri,
                authServerConfig.ExternalLogin.EUSignServicePaths.Certificate),
        };
    }

    private Request CreateDecryptionRequest(EnvelopedUserInfoResponse encryptedUser)
    {
        return new Request
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(
                authServerConfig.ExternalLogin.EUSignServiceUri,
                authServerConfig.ExternalLogin.EUSignServicePaths.Decrypt),
            Data = encryptedUser,
        };
    }

    private Request CreateUserInfoRequest(string remoteUserId, string cert, string backchannelToken)
    {
        return new Request
        {
            HttpMethodType = HttpMethodType.Get,
            Url = new Uri(
                authServerConfig.ExternalLogin.IdServerUri,
                authServerConfig.ExternalLogin.IdServerPaths.UserInfo),
            Token = string.Empty,
            Query = new Dictionary<string, string>
            {
                {AuthServerConstants.ExternalQuery.AccessToken, backchannelToken},
                {AuthServerConstants.ExternalQuery.UserId, remoteUserId},
                {
                    authServerConfig.ExternalLogin.Parameters.UserInfoFields.Key,
                    authServerConfig.ExternalLogin.Parameters.UserInfoFields.EmployeeInfo
                },
                {AuthServerConstants.ExternalQuery.Certificate, Uri.EscapeDataString(cert) },
            },
        };
    }

    private class EncryptionErrorHandler(string? errorMessage) : IErrorHandler<IErrorResponse>
    {
        public Task<IErrorResponse> HandleErrorAsync(CommunicationError errorResponse, string? message = null)
        {
            return Task.FromResult<IErrorResponse>(new ExternalAuthError
            {
                HttpStatusCode = errorResponse.HttpStatusCode,
                Message = errorMessage ?? message,
                Content = errorResponse.Body,
                ErrorGroup = ExternalAuthErrorGroup.Encryption,
            });
        }
    }

    private class IdGovUaErrorHandler : IErrorHandler<IErrorResponse>
    {
        public Task<IErrorResponse> HandleErrorAsync(CommunicationError errorResponse, string? message = null)
        {
            if (errorResponse.HttpStatusCode == HttpStatusCode.Unauthorized)
            {
                IdGovErrorResponse? idGovError;
                try
                {
                    idGovError = JsonSerializerHelper.Deserialize<IdGovErrorResponse>(errorResponse.Body);
                }
                catch (JsonException)
                {
                    return Task.FromResult<IErrorResponse>(new ExternalAuthError
                    {
                        HttpStatusCode = errorResponse.HttpStatusCode,
                        Message = "Invalid error response format.",
                        ErrorGroup = ExternalAuthErrorGroup.Unknown,
                    });
                }

                if (idGovError != null)
                {
                    return Task.FromResult<IErrorResponse>(new ExternalAuthError
                    {
                        HttpStatusCode = errorResponse.HttpStatusCode,
                        Message = idGovError.Error.ToString(),
                        Content = string.Equals(
                            idGovError.Message,
                            idGovError.Description,
                            StringComparison.OrdinalIgnoreCase)
                            ? idGovError.Message
                            : $"{idGovError.Message} - {idGovError.Description}",
                        ErrorGroup = ExternalAuthErrorGroup.IdGovUa,
                    });
                }
            }

            return Task.FromResult<IErrorResponse>(new ExternalAuthError
            {
                HttpStatusCode = errorResponse.HttpStatusCode,
                Message = "Unknown error",
                ErrorGroup = ExternalAuthErrorGroup.Unknown,
            });
        }
    }
}
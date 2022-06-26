using Grpc.Net.Client;
using OutOfSchool.WebApi.Config;

namespace OutOfSchool.WebApi.Services.GRPC
{
    /// <summary>
    /// Defines interface for common operations with GRPC.
    /// </summary>
    public interface IGRPCCommonService
    {
        /// <summary>
        /// Gets a value indicating whether get current GRPC status.
        /// </summary>
        /// <returns>Return true if GRPC is turned on in config.</returns>
        bool IsEnabled { get; }

        /// <summary>
        /// Create authenticated GRPC channel.
        /// </summary>
        /// <param name="token">Authentification token.</param>
        /// <returns>GRPC chanel.</returns>
        GrpcChannel CreateAuthenticatedChannel(string token);
    }
}

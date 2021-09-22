using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DotNext.Threading;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OutOfSchool.IdentityServer.Config;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public class CredentialsStore : ISigningCredentialStore, IValidationKeysStore, IDisposable
    {
        private readonly IServiceScope scope;

        private readonly IssuerConfig config;

        // TODO: Probably should limit to 1 min & 1 max
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private readonly AsyncLazy<X509Certificate2> inMemoryCertificate;

        public CredentialsStore(IOptions<IssuerConfig> options, IServiceProvider services)
        {
            scope = services.CreateScope();
            config = options.Value;
            inMemoryCertificate =
                new AsyncLazy<X509Certificate2>(GetCertificateAsync, true);
        }

        /// <inheritdoc />
        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            var certificate = await GetIfNotExpired();

            return ConvertToCredentials(certificate);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var certificate = await GetIfNotExpired();

            var credential = ConvertToCredentials(certificate);

            var keyInfo = new SecurityKeyInfo
            {
                Key = credential.Key,
                SigningAlgorithm = credential.Algorithm,
            };

            return new[] {keyInfo};
        }

        public void Dispose()
        {
            scope?.Dispose();
        }

        /// <summary>
        /// Converts <see cref="X509Certificate2"/> into a valid <see cref="SigningCredentials"/> for
        /// Identity Server to use for signing and validating tokens.
        /// </summary>
        /// <param name="cert">Application X509 signing certificate.</param>
        /// <param name="signingAlgorithm">Signing algorithm, defaults to SHA256.</param>
        /// <returns>Signing credentials for Identity Server.</returns>
        private static SigningCredentials ConvertToCredentials(
            X509Certificate2 cert,
            string signingAlgorithm = SecurityAlgorithms.RsaSha256)
        {
            var key = new X509SecurityKey(cert);
            key.KeyId += signingAlgorithm;

            return new SigningCredentials(key, signingAlgorithm);
        }

        /// <summary>
        /// Cleans expired certificates from database if any and returns a valid certificate.
        /// If database is empty - creates a new one.
        /// There should be only 1 certificate in database at a time.
        /// </summary>
        /// <returns>An future containing <see cref="X509Certificate2"/>.</returns>
        private async Task<X509Certificate2> GetCertificateAsync()
        {
            await using var dbContext = scope.ServiceProvider.GetRequiredService<CertificateDbContext>();
            var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var certificates = await dbContext.Certificates.ToListAsync();
                foreach (var cert in certificates.Where(cert => cert.ExpirationDate < new DateTimeOffset(DateTime.UtcNow)))
                {
                    dbContext.Certificates.Remove(cert);
                    certificates.Remove(cert);
                }

                await dbContext.SaveChangesAsync();

                var certificate = certificates.FirstOrDefault();
                if (certificate == null)
                {
                    certificate = GenerateSigningCertificate();
                    dbContext.Certificates.Add(certificate);
                    await dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return new X509Certificate2(Convert.FromBase64String(certificate.CertificateBase64));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Generates a new certificate for signing and validation of tokens
        /// </summary>
        /// <param name="certificateName">Friendly name of the certificate.</param>
        /// <returns>Returns <see cref="SigningCertificate" /> that can be save to database.</returns>
        private SigningCertificate GenerateSigningCertificate(string certificateName = "Identity")
        {
            var uri = new Uri(config.Uri);

            var distinguishedName = new X500DistinguishedName($"CN={uri.Host}");

            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                distinguishedName,
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment |
                    X509KeyUsageFlags.DigitalSignature,
                    false));


            // TODO: add a decent oid https://oidref.com/1.3.6.1.5.5.7.3.1
            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection {new Oid("1.3.6.1.5.5.7.3.1")}, false));

            var expirationDate = new DateTimeOffset(DateTime.UtcNow.AddDays(365));

            var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), expirationDate);
            certificate.FriendlyName = certificateName;

            return new SigningCertificate
            {
                CertificateBase64 = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx)),
                ExpirationDate = expirationDate,
            };
        }

        /// <summary>
        /// Returns current caches certificate or forces its re-initialization
        /// </summary>
        /// <returns>An future containing <see cref="X509Certificate2"/>.</returns>
        private async Task<X509Certificate2> GetIfNotExpired()
        {
            await semaphore.WaitAsync();
            try
            {
                var certificate = await inMemoryCertificate;
                if (certificate.NotAfter < DateTime.UtcNow)
                {
                    inMemoryCertificate.Reset();
                    certificate = await inMemoryCertificate;
                }

                return certificate;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
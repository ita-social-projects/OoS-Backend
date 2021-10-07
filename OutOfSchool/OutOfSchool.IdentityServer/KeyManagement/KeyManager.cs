using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.IdentityServer.Config;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public class KeyManager : IKeyManager
    {
        private readonly IServiceScopeFactory scopeFactory;

        private readonly IssuerConfig config;

        private readonly IAppCache inMemoryCertificateCache;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public KeyManager(IOptions<IssuerConfig> options, IServiceScopeFactory factory, IAppCache cache)
        {
            scopeFactory = factory;
            config = options.Value;
            inMemoryCertificateCache = cache;
        }

        /// <inheritdoc />
        public async Task<X509Certificate2> Get()
        {
            var uri = new Uri(config.Uri);

            var certificate = await inMemoryCertificateCache
                .GetOrAddAsync(uri.Host, () => GetCertificateAsync(uri.Host));
            if (certificate.NotAfter < new DateTimeOffset(DateTime.UtcNow))
            {
                // clear and retry
                inMemoryCertificateCache.Remove(uri.Host);
                certificate = await inMemoryCertificateCache
                    .GetOrAddAsync(uri.Host, () => GetCertificateAsync(uri.Host));
            }

            return certificate;
        }

        /// <summary>
        /// Handles the situation where a certificate in DB was updated by other instance of the same service.
        /// </summary>
        /// <param name="e"><see cref="DbUpdateException"/> or <see cref="DbUpdateConcurrencyException"/> depending on error type.</param>
        /// <returns><see cref="X509Certificate2"/> that was updated by other instance.</returns>
        /// <exception cref="Exception">TODO: Figure out exception.</exception>
        private static async Task<X509Certificate2> HandleConcurrentUpdate(DbUpdateException e)
        {
            var entry = e.Entries.Single();
            var databaseValues = await entry.GetDatabaseValuesAsync();
            if (databaseValues == null)
            {
                // Data was deleted but we didn't do it, this case should be unreal
                // throw exception and hope for the better next time
                throw new Exception();
            }

            // Certificate was updated by other instance
            var cert = (SigningCertificate) databaseValues.ToObject();

            // it is valid -> return
            if (cert.ExpirationDate >= new DateTimeOffset(DateTime.UtcNow))
            {
                return new X509Certificate2(Convert.FromBase64String(cert.CertificateBase64));
            }

            // Updated and expired right away, this case should be unreal
            // throw exception and hope for the better next time?
            throw new Exception();
        }

        /// <summary>
        /// Updates expired certificate in database if exists and returns a valid certificate.
        /// If database is empty - creates a new one.
        /// There should be only 1 certificate in database at a time for a given hostname.
        /// </summary>
        /// <param name="hostname">Hostname key for certificate.</param>
        /// <returns>An future containing <see cref="X509Certificate2"/>.</returns>
        private async Task<X509Certificate2> GetCertificateAsync(string hostname)
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CertificateDbContext>();

            var certificate = await dbContext.Certificates.FindAsync(hostname);

            // Certificate exists and valid -> return
            if (certificate != null && certificate.ExpirationDate >= new DateTimeOffset(DateTime.UtcNow))
            {
                return new X509Certificate2(Convert.FromBase64String(certificate.CertificateBase64));
            }

            // Certificate exists and expired -> generate & update
            if (certificate != null)
            {
                var replacementCertificate = GenerateSigningCertificate();

                try
                {
                    await semaphore.WaitAsync();
                    dbContext
                        .Entry(certificate)
                        .CurrentValues
                        .SetValues(replacementCertificate);
                    await dbContext.SaveChangesAsync();

                    // return updated certificate
                    return new X509Certificate2(Convert.FromBase64String(replacementCertificate.CertificateBase64));
                }
                catch (DbUpdateConcurrencyException e)
                {
                    return await HandleConcurrentUpdate(e);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            // certificate does not exist
            var newCertificate = GenerateSigningCertificate();
            try
            {
                await semaphore.WaitAsync();
                dbContext.Add(newCertificate);
                await dbContext.SaveChangesAsync();

                // return new certificate
                return new X509Certificate2(Convert.FromBase64String(newCertificate.CertificateBase64));
            }
            catch (DbUpdateException e)
            {
                return await HandleConcurrentUpdate(e);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Generates a new certificate for signing and validation of tokens.
        /// </summary>
        /// <returns>Returns <see cref="SigningCertificate" /> that can be save to database.</returns>
        private SigningCertificate GenerateSigningCertificate()
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

            var expirationDate = new DateTimeOffset(DateTime.UtcNow.AddDays(config.CertificateExpirationDays));

            var certificate = request.CreateSelfSigned(
                new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
                expirationDate);

            return new SigningCertificate
            {
                Id = uri.Host,
                CertificateBase64 = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx)),
                ExpirationDate = expirationDate,
            };
        }
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using EUSignCP;
using Microsoft.Extensions.Options;
using OutOfSchool.Encryption.Config;
using OutOfSchool.Encryption.Constants;
using OutOfSchool.Encryption.Models;

namespace OutOfSchool.Encryption.Services;

/// <inheritdoc/>
public class EUSignOAuth2Service : IEUSignOAuth2Service
{
    private readonly EUSignConfig eUSignConfig;
    private readonly ILogger<EUSignOAuth2Service> logger;
    private readonly IIoOperationsService ioOperationsService;

    // ReSharper disable once InconsistentNaming
    [SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1306:Field names should begin with lower-case letter",
        Justification = "CA is an acronym")]
    private CASettings[] CAs = null;

    private IntPtr context = IntPtr.Zero;
    private IntPtr pkContext = IntPtr.Zero;
    private byte[] pkSignCert = null;
    private byte[] pkEnvelopeCert = null;

    public EUSignOAuth2Service(
        IOptions<EUSignConfig> config,
        IIoOperationsService ioOperationsService,
        ILogger<EUSignOAuth2Service> logger)
    {
        this.eUSignConfig = config.Value;
        this.ioOperationsService = ioOperationsService;
        this.logger = logger;

        if (!IsInitialized())
        {
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                // No logging, as this will lead to server crash and log at that point
                throw new EUSignOAuth2Exception("An error occurred while initializing the cryptographic library.", ex);
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }

        if (!IsPrivateKeyLoaded())
        {
            try
            {
                ReadPrivateKey(
                    eUSignConfig.PrivateKey.FileName,
                    eUSignConfig.PrivateKey.MediaType,
                    eUSignConfig.PrivateKey.MediaDevice,
                    eUSignConfig.PrivateKey.Password,
                    eUSignConfig.PrivateKey.JKSAlias,
                    eUSignConfig.PrivateKey.CertificateFilePaths,
                    eUSignConfig.PrivateKey.CAIssuerCN);
            }
            catch (Exception ex)
            {
                // No logging, as this will lead to server crash and log at that point
                throw new EUSignOAuth2Exception("An error occurred while reading the server's private key.", ex);
            }
        }
    }

    /// <inheritdoc/>
    public CertificateResponse GetEnvelopeCertificateBase64()
    {
        if (pkEnvelopeCert == null)
        {
            logger.LogError("No envelop certificate was loaded");
            return null;
        }

        return new()
        {
            CertBase64 = Convert.ToBase64String(pkEnvelopeCert),
        };
    }

    /// <inheritdoc/>
    public UserInfoResponse DecryptUserInfo([NotNull] EnvelopedUserInfoResponse encryptedUserInfo)
    {
        if (encryptedUserInfo == null)
        {
            return null;
        }

        try
        {
            IEUSignCP.CtxDevelopData(
                pkContext,
                encryptedUserInfo.EncryptedUserInfo,
                null,
                out var developedUserInfo,
                out _);

            using var userInfoStream = ioOperationsService.GetMemoryStreamFromBytes(developedUserInfo);
            return JsonSerializer.Deserialize<UserInfoResponse>(userInfoStream);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while decrypting user info");
            return null;
        }
    }

    /// <summary>
    /// Пошук зареєстрованого носія ключової інформації
    /// This code is from IIT Library usage example with minimal code styling changes.
    /// </summary>
    private static void GetKeyMedia(
        string type,
        string device,
        string password,
        out IEUSignCP.EU_KEY_MEDIA keyMedia)
    {
        keyMedia = new IEUSignCP.EU_KEY_MEDIA(0, 0, string.Empty);

        try
        {
            string curType, curDevice;

            for (var typeIndex = 0; typeIndex <= Int32.MaxValue; typeIndex++)
            {
                IEUSignCP.EnumKeyMediaTypes(typeIndex, out curType);
                if (curType == type)
                {
                    for (var deviceIndex = 0; deviceIndex <= Int32.MaxValue; deviceIndex++)
                    {
                        IEUSignCP.EnumKeyMediaDevices(
                            typeIndex, deviceIndex, out curDevice);
                        if (curDevice == device)
                        {
                            keyMedia = new IEUSignCP.EU_KEY_MEDIA(
                                typeIndex, deviceIndex, password);
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            throw new EUSignOAuth2Exception(
                $"""
                 An error occurred while searching for key information media.
                 Error description: media not found.
                 Search parameters: media type - {type}, device - {device}
                 """);
        }
    }

    /// <summary>
    /// This code is from IIT Library usage example with minimal code styling changes.
    /// </summary>
    private bool IsInitialized()
    {
        return context != IntPtr.Zero;
    }

    /// <summary>
    /// This code is from IIT Library usage example with minimal code styling changes.
    /// </summary>
    private void Initialize()
    {
        // Увімкнення кидання винятків бібліотекою, замість повертання помилок.
        IEUSignCP.SetThrowExceptions(true);

        // Вимкнення завантаження бібліотеки граф. діалогів
        IEUSignCP.SetUIMode(false);

        // Ініціалізація бібліотеки
        IEUSignCP.Initialize();

        // Вимкнення збереження налаштувань до реєстру
        IEUSignCP.SetRuntimeParameter(
            IEUSignCP.EU_SAVE_SETTINGS_PARAMETER, IEUSignCP.EU_SETTINGS_ID_NONE);

        // Вимкнення відображення помилок
        IEUSignCP.SetUIMode(false);

        // Встановлення параметрів взаємодії з сервісами TSP-, OCSP-, CMP- ЦСК
        // Використання режиму он-лайн
        IEUSignCP.SetModeSettings(false);

        // Встановлення налаштувань Сховища сертифікатів та СВС
        IEUSignCP.SetFileStoreSettings(
            eUSignConfig.FileStore.Path,
            eUSignConfig.FileStore.CheckCRLs,
            eUSignConfig.FileStore.AutoRefresh,
            eUSignConfig.FileStore.OwnCRLsOnly,
            eUSignConfig.FileStore.FullAndDeltaCRLs,
            eUSignConfig.FileStore.AutoDownloadCRLs,
            eUSignConfig.FileStore.SaveLoadedCerts,
            eUSignConfig.FileStore.ExpireTime);

        if (Uri.CheckHostName(eUSignConfig.Proxy.Host) is UriHostNameType.Unknown or UriHostNameType.Basic)
        {
            throw new ArgumentException("The proxy address is invalid.", nameof(eUSignConfig.Proxy.Host));
        }

        // Встановлення параметрів Proxy-серверу для доступу к серверам ЦСК
        IEUSignCP.SetProxySettings(
            eUSignConfig.Proxy.Enabled,
            eUSignConfig.Proxy.User == string.Empty,
            eUSignConfig.Proxy.Host,
            eUSignConfig.Proxy.Port.ToString(),
            eUSignConfig.Proxy.User,
            eUSignConfig.Proxy.Password,
            eUSignConfig.Proxy.SavePassword);

        // Встановлення параметрів OCSP-серверів для перевірки сертифікатів підписувачів
        // Порядок використання параметрів OCSP:
        // - з точки доступу до OCSP, сертифіката що перевіряється;
        // - з точки доступу до OCSP, з налаштувань бібліотеки;
        // - з параметрів OCSP, що встановлені за замовчанням.

        // Встановлення параметрів OCSP-серверу ЦСК за замовчанням
        IEUSignCP.SetOCSPSettings(
            true, true, eUSignConfig.DefaultOCSPServer, eUSignConfig.DefaultOCSPPort.ToString());

        // Встановлення налаштувань точок доступу до OCSP-серверів
        // Необхідні при обслуговуванні користувачів з різних ЦСК
        IEUSignCP.SetOCSPAccessInfoModeSettings(true);

        using var fsCAs = ioOperationsService.GetFileStreamFromPath(eUSignConfig.CA.JsonPath, FileMode.OpenOrCreate);

        CAs = JsonSerializer.Deserialize<CASettings[]>(fsCAs);

        foreach (var ca in CAs)
        {
            foreach (var commonName in ca.IssuerCNs)
            {
                IEUSignCP.SetOCSPAccessInfoSettings(
                    commonName,
                    ca.OcspAccessPointAddress,
                    ca.OcspAccessPointPort);
            }
        }

        // Встановлення параметрів TSP-серверу ЦСК
        // Параметри використовуються при накладанні підпису для додавання мітки часу
        // Порядок використання параметрів TSP: 
        // - з точки доступу до TSP, сертифіката підписувача;
        // - з параметрів TSP, що встановлені за замовчанням.

        // Встановлення параметрів TSP-серверу ЦСК за замовчанням
        IEUSignCP.SetTSPSettings(true, eUSignConfig.DefaultTSPServer, eUSignConfig.DefaultTSPPort.ToString());

        // Встановлення налаштувань LDAP-cервера
        IEUSignCP.SetLDAPSettings(eUSignConfig.LDAP.Enabled, eUSignConfig.LDAP.Host, eUSignConfig.LDAP.Port.ToString(), eUSignConfig.LDAP.User == string.Empty, eUSignConfig.LDAP.User, eUSignConfig.LDAP.Password);

        // Встановлення параметрів CMP-серверу ЦСК
        IEUSignCP.SetCMPSettings(eUSignConfig.CMP.Enabled, eUSignConfig.CMP.Host, eUSignConfig.CMP.Port.ToString(), eUSignConfig.CMP.CommonName);

        // Збереження кореневих сертифікатів ЦЗО та ЦСК
        IEUSignCP.SaveCertificates(
            File.ReadAllBytes(eUSignConfig.CA.CertificatesPath));

        // Створення контексту бібліотеки
        IEUSignCP.CtxCreate(out context);
    }

    /// <summary>
    /// This code is from IIT Library usage example with code styling changes.
    /// Original code:
    /// foreach (var ca in CAs)
    /// {
    ///     foreach (var issuer in ca.issuerCNs)
    ///     {
    ///         if (caSubjectCN == issuer)
    ///         {
    ///             return ca;
    ///         }
    ///     }
    /// }
    ///
    /// return null;
    /// Returns CA parameters by subject name.
    /// </summary>
    /// <param name="caSubjectCN">Required CS Subject Common Name.</param>
    /// <returns>A <see cref="CASettings"/> with required CA parameters.</returns>
    private CASettings GetCA(string caSubjectCN) => CAs.FirstOrDefault(ca => ca.IssuerCNs.Any(cn => cn == caSubjectCN));

    /// <summary>
    /// This code is from IIT Library usage example with minimal code styling changes.
    /// </summary>
    private bool IsPrivateKeyLoaded()
    {
        return pkContext != IntPtr.Zero;
    }

    /// <summary>
    /// This code is from IIT Library usage example with minimal code styling changes.
    /// </summary>
    private void ReadPrivateKey(
        string pkFile,
        string pkTypeName,
        string pkDeviceName,
        string password,
        string jksAlias,
        string[] certsFiles,
        string caIssuerCN)
    {
        byte[] pKey = null;
        List<byte[]> certs = new List<byte[]>();
        bool fromFile = !string.IsNullOrEmpty(pkFile);
        IEUSignCP.EU_KEY_MEDIA keyMedia = new IEUSignCP.EU_KEY_MEDIA(0, 0, string.Empty);
        CASettings ca = null;

        try
        {
            // Зчитування сертифікатів ос. ключа, з файлової системи (якщо задані)
            if (certsFiles != null && certsFiles.Length > 0)
            {
                foreach (var file in certsFiles)
                {
                    if (!ioOperationsService.Exists(file))
                    {
                        throw new Exception($"The certificate file is missing: {file}");
                    }

                    certs.Add(ioOperationsService.ReadAllBytes(file));
                }
            }

            if (fromFile)
            {
                // Зчитування ос. ключа з файлу до байтового масиву
                if (!ioOperationsService.Exists(pkFile))
                {
                    throw new Exception($"The private key file is missing: {pkFile}");
                }

                pKey = ioOperationsService.ReadAllBytes(pkFile);
                if (!string.IsNullOrEmpty(jksAlias))
                {
                    // Пошук відповідного ос. ключа в контейнері JKS
                    IEUSignCP.GetJKSPrivateKey(pKey, jksAlias, out pKey, out var certificates);
                    certs.AddRange(certificates);
                }
            }
            else
            {
                // Отримання параметрів ключового носія
                GetKeyMedia(pkTypeName, pkDeviceName, password, out keyMedia);
            }

            IEUSignCP.EU_CERT_INFO_EX infoEx;
            if (certs.Count == 0 && !string.IsNullOrEmpty(caIssuerCN))
            {
                // Пошук налаштувань ЦСК для ключа
                ca = GetCA(caIssuerCN);
                if (ca == null)
                {
                    throw new Exception($"CA settings not found: {caIssuerCN}");
                }

                if (ca.CmpAddress != string.Empty)
                {
                    byte[] keyInfo;
                    byte[] certsCMP;
                    byte[] cert;

                    // Завантаження сертифікатів користувача з CMP-серверу ЦСК
                    if (fromFile)
                    {
                        IEUSignCP.GetKeyInfoBinary(pKey, password, out keyInfo);
                    }
                    else
                    {
                        IEUSignCP.GetKeyInfo(keyMedia, out keyInfo);
                    }

                    string[] cmpServers = { ca.CmpAddress };
                    string[] cmpServersPorts = { AppConstants.DefaultPort.ToString() };
                    IEUSignCP.GetCertificatesByKeyInfo(
                        keyInfo, cmpServers, cmpServersPorts, out certsCMP);

                    for (var i = 0; i <= Int32.MaxValue; i++)
                    {
                        try
                        {
                            IEUSignCP.GetCertificateFromSignedData(
                                i, certsCMP, out infoEx, out cert);
                            if (infoEx.subjectType == IEUSignCP.EU_SUBJECT_TYPE.END_USER)
                            {
                                certs.Add(cert);
                            }
                        }
                        catch (IEUSignCP.EUSignCPException ex)
                        {
                            if (ex.errorCode != IEUSignCP.EU_WARNING_END_OF_ENUM)
                            {
                                throw;
                            }

                            break;
                        }
                    }
                }
            }

            // Збереження сертифікатів ос. ключа
            foreach (var cert in certs)
            {
                IEUSignCP.SaveCertificate(cert);
            }

            // Зчитування ос. ключа
            if (fromFile)
            {
                IEUSignCP.CtxReadPrivateKeyBinary(
                    context,
                    pKey,
                    password,
                    out _,
                    out pkContext);
            }
            else
            {
                IEUSignCP.CtxReadPrivateKey(
                    context,
                    keyMedia,
                    out _,
                    out pkContext);
            }

            try
            {
                // Отримання сертифікату для підпису
                IEUSignCP.CtxGetOwnCertificate(
                    pkContext,
                    IEUSignCP.EU_CERT_KEY_TYPE_DSTU4145,
                    IEUSignCP.EU_KEY_USAGE_DIGITAL_SIGNATURE,
                    out infoEx,
                    out pkSignCert);
            }
            catch (Exception)
            {
                throw new Exception("The private key does not support signing described in ДСТУ 4145");
            }

            try
            {
                // Отримання сертифікату для направленого шифрування
                IEUSignCP.CtxGetOwnCertificate(
                    pkContext,
                    IEUSignCP.EU_CERT_KEY_TYPE_DSTU4145,
                    IEUSignCP.EU_KEY_USAGE_KEY_AGREEMENT,
                    out infoEx,
                    out pkEnvelopeCert);
            }
            catch (Exception)
            {
                throw new Exception("The private key does not support PKI encryption described in ДСТУ 4145");
            }
        }
        catch (Exception)
        {
            if (pkContext != IntPtr.Zero)
            {
                IEUSignCP.CtxFreePrivateKey(pkContext);
                pkContext = IntPtr.Zero;
            }

            throw;
        }
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using EUSignCP;
using Microsoft.Extensions.Options;
using OutOfSchool.Encryption.Config;
using OutOfSchool.Encryption.Models;

namespace OutOfSchool.Encryption.Services;

public class EUSignOAuth2Service : IEUSignOAuth2Service
{
    private readonly EUSignConfig eUSignConfig;
    private readonly Logger<EUSignOAuth2Service> logger;

    // ReSharper disable once InconsistentNaming
    [SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1306:Field names should begin with lower-case letter",
        Justification = "CA is an acronym")]
    private CASettings[] CAs = null;
    private IntPtr context = IntPtr.Zero;
    private IntPtr pkContext = IntPtr.Zero;
    private byte[] pkSignCert = null;
    private byte[] pkEnvelopCert = null;

    public EUSignOAuth2Service(
        IOptions<EUSignConfig> config,
        Logger<EUSignOAuth2Service> logger)
    {
        this.eUSignConfig = config.Value;
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

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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
    public CertificateResponse GetEnvelopCertificateBase64()
    {
        if (pkEnvelopCert == null)
        {
            logger.LogError("No envelop certificate was loaded");
            return null;
        }

        return new()
        {
            CertBase64 = Convert.ToBase64String(pkEnvelopCert),
        };
    }

    /// <inheritdoc/>
    public UserInfoResponse DecryptUserInfo(EnvelopedUserInfoResponse encryptedUserInfo)
    {
        try
        {
            IEUSignCP.CtxDevelopData(
                pkContext,
                encryptedUserInfo.EncryptedUserInfo,
                null,
                out var developedUserInfo,
                out _);

            Stream userInfoStream = new MemoryStream(developedUserInfo);
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
            int typeIndex, deviceIndex;
            string curType, curDevice;

            typeIndex = 0;
            while (true)
            {
                IEUSignCP.EnumKeyMediaTypes(typeIndex, out curType);
                if (curType == type)
                {
                    deviceIndex = 0;

                    while (true)
                    {
                        IEUSignCP.EnumKeyMediaDevices(
                            typeIndex, deviceIndex, out curDevice);
                        if (curDevice == device)
                        {
                            keyMedia = new IEUSignCP.EU_KEY_MEDIA(
                                typeIndex, deviceIndex, password);
                            return;
                        }

                        deviceIndex++;
                    }
                }

                typeIndex++;
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
            string.Empty,
            false,
            true,
            false,
            false,
            false,
            false,
            3600);

        // Встановлення параметрів Proxy-серверу для доступу к серверам ЦСК
        IEUSignCP.SetProxySettings(
            eUSignConfig.Proxy.Use,
            eUSignConfig.Proxy.User != string.Empty,
            eUSignConfig.Proxy.Address,
            eUSignConfig.Proxy.Port,
            eUSignConfig.Proxy.User,
            eUSignConfig.Proxy.Password,
            true);

        // Встановлення параметрів OCSP-серверів для перевірки сертифікатів підписувачів
        // Порядок використання параметрів OCSP:
        // - з точки доступу до OCSP, сертифіката що перевіряється;
        // - з точки доступу до OCSP, з налаштувань бібліотеки;
        // - з параметрів OCSP, що встановлені за замовчанням.

        // Встановлення параметрів OCSP-серверу ЦСК за замовчанням
        IEUSignCP.SetOCSPSettings(
            true, true, eUSignConfig.DefaultOCSPServer, "80");

        // Встановлення налаштувань точок доступу до OCSP-серверів
        // Необхідні при обслуговуванні користувачів з різних ЦСК
        IEUSignCP.SetOCSPAccessInfoModeSettings(true);

        var fsCAs = new FileStream(eUSignConfig.CA.JsonPath, FileMode.OpenOrCreate);

        CAs = JsonSerializer.Deserialize<CASettings[]>(fsCAs);

        foreach (var ca in CAs)
        {
            foreach (var commonName in ca.issuerCNs)
            {
                IEUSignCP.SetOCSPAccessInfoSettings(
                    commonName,
                    ca.ocspAccessPointAddress,
                    ca.ocspAccessPointPort);
            }
        }

        // Встановлення параметрів TSP-серверу ЦСК
        // Параметри використовуються при накладанні підпису для додавання мітки часу
        // Порядок використання параметрів TSP: 
        // - з точки доступу до TSP, сертифіката підписувача;
        // - з параметрів TSP, що встановлені за замовчанням.

        // Встановлення параметрів TSP-серверу ЦСК за замовчанням
        IEUSignCP.SetTSPSettings(true, eUSignConfig.DefaultTSPServer, "80");

        // Встановлення налаштувань LDAP-cервера
        IEUSignCP.SetLDAPSettings(false, string.Empty, string.Empty, true, string.Empty, string.Empty);

        // Встановлення параметрів CMP-серверу ЦСК
        IEUSignCP.SetCMPSettings(false, string.Empty, "80", string.Empty);

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
    private CASettings GetCA(string caSubjectCN) => CAs.FirstOrDefault(ca => ca.issuerCNs.Any(cn => cn == caSubjectCN));

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
        IEUSignCP.EU_CERT_OWNER_INFO info;
        IEUSignCP.EU_CERT_INFO_EX infoEx;

        try
        {
            // Зчитування сертифікатів ос. ключа, з файлової системи (якщо задані)
            if (certsFiles != null && certsFiles.Length > 0)
            {
                foreach (var file in certsFiles)
                {
                    if (!File.Exists(file))
                    {
                        throw new Exception($"The certificate file is missing: {file}");
                    }

                    certs.Add(File.ReadAllBytes(file));
                }
            }

            if (fromFile)
            {
                // Зчитування ос. ключа з файлу до байтового масиву
                if (!File.Exists(pkFile))
                {
                    throw new Exception($"The private key file is missing: {pkFile}");
                }

                pKey = File.ReadAllBytes(pkFile);
                if (!string.IsNullOrEmpty(jksAlias))
                {
                    // Пошук відповідного ос. ключа в контейнері JKS
                    IEUSignCP.GetJKSPrivateKey(pKey, jksAlias, out pKey, out var certificates);
                    foreach (var certificate in certificates)
                    {
                        certs.Add(certificate);
                    }
                }
            }
            else
            {
                // Отримання параметрів ключового носія
                GetKeyMedia(pkTypeName, pkDeviceName, password, out keyMedia);
            }

            if (certs.Count == 0 && !string.IsNullOrEmpty(caIssuerCN))
            {
                // Пошук налаштувань ЦСК для ключа
                ca = GetCA(caIssuerCN);
                if (ca == null)
                {
                    throw new Exception($"CA settings not found: {caIssuerCN}");
                }

                if (ca.cmpAddress != string.Empty)
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

                    string[] cmpServers = { ca.cmpAddress };
                    string[] cmpServersPorts = { "80" };
                    IEUSignCP.GetCertificatesByKeyInfo(
                        keyInfo, cmpServers, cmpServersPorts, out certsCMP);

                    int i = 0;
                    while (true)
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

                        i++;
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
                    out info,
                    out pkContext);
            }
            else
            {
                IEUSignCP.CtxReadPrivateKey(
                    context,
                    keyMedia,
                    out info,
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
                    out pkEnvelopCert);
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
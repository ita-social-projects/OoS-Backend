namespace OutOfSchool.Encryption.Config;

public class FileStore
{
    public string Path { get; set; }

    public bool CheckCRLs { get; set; }

    public bool AutoRefresh { get; set; }

    public bool OwnCRLsOnly { get; set; }

    public bool FullAndDeltaCRLs { get; set; }

    public bool AutoDownloadCRLs { get; set; }

    public bool SaveLoadedCerts { get; set; }

    public int ExpireTime { get; set; }
}
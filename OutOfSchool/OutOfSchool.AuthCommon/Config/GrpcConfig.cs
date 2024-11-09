namespace OutOfSchool.AuthCommon.Config;

public class GrpcConfig
{
    public const string Name = "GRPC";

    public bool Enabled { get; set; }

    public int Port { get; set; }

    public string EmployeeConfirmationLink { get; set; } = null!;
}
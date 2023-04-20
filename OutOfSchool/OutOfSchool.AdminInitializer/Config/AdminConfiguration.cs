namespace OutOfSchool.AdminInitializer.Config;

public class AdminConfiguration
{
    public static readonly string Name = "AdminUser";

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string Password { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string Role { get; set; }

    public bool Reset { get; set; }
}
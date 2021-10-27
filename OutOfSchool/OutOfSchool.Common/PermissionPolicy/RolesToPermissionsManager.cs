namespace OutOfSchool.Common.PermissionsModule
{
    // mock permission for role calculator.
    public class RolesToPermissionsManager
    {
        internal static string CalcPermissions(string role)
        {
            string result = string.Empty;
            switch (role)
            {
                case "admin":
                    result = "AccessAll";
                    break;

                case "provider":
                    result = "WorkshopAll";
                    break;

                case "parent":
                    result = "ChildrenAll";
                    break;
            }

            return result;
        }
    }
}


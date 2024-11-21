namespace OutOfSchool.AuthCommon.Config;

public class UserInfoFields
{
    public string Key { get; set; }
    
    public List<string> PersonalInfo { get; set; }
    
    public List<string> BusinessInfo { get; set; }
    
    public List<string> TechnicalInfo { get; set; }
    
    public string EmployeeInfo => string.Join(",", [..PersonalInfo, ..BusinessInfo]);
    
    public string FullInfo => string.Join(",", [..PersonalInfo, ..BusinessInfo, ..TechnicalInfo]);
}
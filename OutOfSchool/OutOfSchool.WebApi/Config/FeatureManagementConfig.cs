namespace OutOfSchool.WebApi.Config;

public class FeatureManagementConfig
{
    public const string Name = "FeatureManagement";

    public bool Release1 { get; set; }

    public bool Release2 { get; set; }

    public bool Release3 { get; set; }

    public bool ShowForProduction { get; set; }

    public bool TechAdminImport { get; set; }

    public bool Images { get; set; }

    public bool TechAdminExport { get; set; }

    public bool EnableWorkshopGroupTypeField { get; set; }

    public bool DirectionManagement { get; set; }

    public bool AchievementManagement { get; set; }

    public bool AdminsChildrenParentsManagement { get; set; }

    public bool MessagingFeature { get; set; }

    public bool PasswordLogin { get; set; }
    
    public bool PasswordRegistration { get; set; }
    
    public bool EmailConfirmation { get; set; }
    
    public bool EmailManagement { get; set; }
    
    public bool PasswordManagement { get; set; }

    public bool OnlyUkrainianLanguage {  get; set; }
}
    

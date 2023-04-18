using System;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.Services.Models;

public class StatisticReportCSV : IKeyedEntity<int>
{
    [Required]
    [Ignore]
    public int Id { get; set; }

    [Name("Рік звітності")]
    public int Year { get; set; }

    [Name("ID закладу позашкільної освіти")]
    public Guid ProviderId { get; set; }

    [Name("Повна назва закладу")]
    public string ProviderName { get; set; }

    [Name("ЄДРПОУ")]
    public string EDRPOU { get; set; }

    [Name("Форма власності")]
    public string ProviderType { get; set; }

    [Name("Підпорядкування(Назва міністерства)")]
    public string InstitutionTitle { get; set; }

    [Name("КАТОТТГ")]
    public string CATOTTGCode { get; set; }

    [Name("Область")]
    public string Region { get; set; }

    [Name("Громада")]
    public string TerritorialCommunity { get; set; }

    [Name("Населений пункт")]
    public string Settlement { get; set; }

    [Name("Тип місцевості")]
    public string CATOTTGCategory { get; set; }

    [Name("Комплексний/профільний")]
    public string Complex { get; set; }

    [Name("Статус(окрема юридична особа/структурний підрозділ)")]
    public string Status { get; set; }

    [Name("Загальна кількість гуртків")]
    public int WorkshopsAmount { get; set; }

    [Name("Загальна кількість поданих заяв(всі які є для цього надавача, за звітний період)")]
    public int ApplicationsAmount { get; set; }

    [Name("Загальна кількість зарахованих заяв(заразовані/навчаються, за звітній період)")]
    public int ApplicationsApproved { get; set; }

    [Name("Загальна кількість здобувачів освіти(загалом, крім тих хто завершив навчання)")]
    public int ChildrenStudying { get; set; }

    [Name("Загальна кількість здобувачів освіти жіночої статі(загалом, крім тих хто завершив навчання)")]
    public int ChildrenStudyingFemale { get; set; }

    [Name("Загальна кількість здобувачів освіти віком до 18 років(загалом, крім тих хто завершив навчання)")]
    public int ChildrenStudyingLess18 { get; set; }

    [Name("Загальна кількість викладачів")]
    public int Teachers { get; set; }

    // By InstitutionHierarchy
    [Name("Кількість гуртків (в розрізі напрямків)")]
    public int WorkshopsAmountInstitutionHierarchy { get; set; }

    [Name("Кількість здобувачів освіти (в розрізі напрямків)")]
    public int ChildrenStudyingInstitutionHierarchy { get; set; }

    [Name("Кількість здобувачів освіти жіночої статі (в розрізі напрямків)")]
    public int ChildrenStudyingFemaleInstitutionHierarchy { get; set; }

    [Name("Кількість здобувачів освіти із досягненнями (в розрізі напрямків)")]
    public int ChildrenStudyingAchievementsInstitutionHierarchy { get; set; }

    [Name("Кількість здобувачів освіти із багатодітних сімей (в розрізі напрямків)")]
    public int ChildrenStudyingLargeFamilyInstitutionHierarchy { get; set; }

    [Name("Кількість здобувачів освіти із малозабезпечених сімей (в розрізі напрямків)")]
    public int ChildrenStudyingPoorFamilyInstitutionHierarchy { get; set; }

    [Name("Кількість здобувачів освіти з інвалідністю (в розрізі напрямків)")]
    public int ChildrenStudyingDisabilityInstitutionHierarchy { get; set; }

    [Name("Кількість здобувачів освіти, що є сиротами або позбавленими батьківського піклування (в розрізі напрямків)")]
    public int ChildrenStudyingOrphanInstitutionHierarchy { get; set; }

    [Name("Загальна кількість викладачів (в розрізі напрямків)")]
    public int TeachersInstitutionHierarchy { get; set; }

    [Name("Кількість викладачів віком до 30 (в розрізі напрямків)")]
    public int TeachersLess30InstitutionHierarchy { get; set; }

    [Name("Кількість викладачів віком 31 - 40 (в розрізі напрямків)")]
    public int TeachersFrom31To40InstitutionHierarchy { get; set; }

    [Name("Кількість викладачів віком 41 - 50 (в розрізі напрямків)")]
    public int TeachersFrom41To50InstitutionHierarchy { get; set; }

    [Name("Кількість викладачів віком 51 - 55 (в розрізі напрямків)")]
    public int TeachersFrom51To55InstitutionHierarchy { get; set; }

    [Name("Кількість викладачів віком 55 + (в розрізі напрямків)")]
    public int TeachersFrom55InstitutionHierarchy { get; set; }
}

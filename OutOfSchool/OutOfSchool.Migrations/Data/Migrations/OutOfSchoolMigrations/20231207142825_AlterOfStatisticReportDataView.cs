using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class AlterOfStatisticReportDataView : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP VIEW IF EXISTS `outofschool`.`statisticreportdata`;");
        migrationBuilder.Sql(@"
    CREATE 
    VIEW `outofschool`.`statisticreportdata` AS
    SELECT 
        ROW_NUMBER() OVER () AS `Id`,
        `firstquery`.`Year` AS `Year`,
        `firstquery`.`ProviderId` AS `ProviderId`,
        `firstquery`.`ProviderName` AS `ProviderName`,
        `firstquery`.`EDRPOU` AS `EDRPOU`,
        `firstquery`.`ProviderType` AS `ProviderType`,
        `firstquery`.`InstitutionTitle` AS `InstitutionTitle`,
        `firstquery`.`CATOTTGCode` AS `CATOTTGCode`,
        `firstquery`.`CATOTTGCategory` AS `CATOTTGCategory`,
        `firstquery`.`Region` AS `Region`,
        `firstquery`.`TerritorialCommunity` AS `TerritorialCommunity`,
        `firstquery`.`Settlement` AS `Settlement`,
        `firstquery`.`Complex` AS `Complex`,
        `firstquery`.`Status` AS `Status`,
        `firstquery`.`WorkshopsAmount` AS `WorkshopsAmount`,
        `firstquery`.`ApplicationsAmount` AS `ApplicationsAmount`,
        `firstquery`.`ApplicationsApproved` AS `ApplicationsApproved`,
        `firstquery`.`ChildrenStudying` AS `ChildrenStudying`,
        `firstquery`.`ChildrenStudyingFemale` AS `ChildrenStudyingFemale`,
        `firstquery`.`ChildrenStudyingLess18` AS `ChildrenStudyingLess18`,
        `firstquery`.`Teachers` AS `Teachers`,
        IFNULL(`secondquery`.`InstitutionHierarchyTitle`,
                '') AS `InstitutionHierarchyTitle`,
        IFNULL(`secondquery`.`WorkshopsAmountInstitutionHierarchy`,
                0) AS `WorkshopsAmountInstitutionHierarchy`,
        IFNULL(`secondquery`.`ChildrenStudyingInstitutionHierarchy`,
                0) AS `ChildrenStudyingInstitutionHierarchy`,
        IFNULL(`secondquery`.`ChildrenStudyingFemaleInstitutionHierarchy`,
                0) AS `ChildrenStudyingFemaleInstitutionHierarchy`,
        IFNULL(`secondquery`.`ChildrenStudyingLess18`,
                0) AS `ChildrenStudyingLess18InstitutionHierarchy`,
        IFNULL(`secondquery`.`ChildrenStudyingAchievementsInstitutionHierarchy`,
                0) AS `ChildrenStudyingAchievementsInstitutionHierarchy`,
        IFNULL(`secondquery`.`ChildrenStudyingLargeFamilyInstitutionHierarchy`,
                0) AS `ChildrenStudyingLargeFamilyInstitutionHierarchy`,
        IFNULL(`secondquery`.`ChildrenStudyingPoorFamilyInstitutionHierarchy`,
                0) AS `ChildrenStudyingPoorFamilyInstitutionHierarchy`,
        IFNULL(`secondquery`.`ChildrenStudyingDisabilityInstitutionHierarchy`,
                0) AS `ChildrenStudyingDisabilityInstitutionHierarchy`,
        IFNULL(`secondquery`.`ChildrenStudyingOrphanInstitutionHierarchy`,
                0) AS `ChildrenStudyingOrphanInstitutionHierarchy`,
        IFNULL(`secondquery`.`TeachersInstitutionHierarchy`,
                0) AS `TeachersInstitutionHierarchy`,
        IFNULL(`secondquery`.`TeachersLess30InstitutionHierarchy`,
                0) AS `TeachersLess30InstitutionHierarchy`,
        IFNULL(`secondquery`.`TeachersFrom31To40InstitutionHierarchy`,
                0) AS `TeachersFrom31To40InstitutionHierarchy`,
        IFNULL(`secondquery`.`TeachersFrom41To50InstitutionHierarchy`,
                0) AS `TeachersFrom41To50InstitutionHierarchy`,
        IFNULL(`secondquery`.`TeachersFrom51To55InstitutionHierarchy`,
                0) AS `TeachersFrom51To55InstitutionHierarchy`,
        IFNULL(`secondquery`.`TeachersFrom55InstitutionHierarchy`,
                0) AS `TeachersFrom55InstitutionHierarchy`
    FROM
        ((SELECT 
            YEAR((CURDATE() + INTERVAL -(1) DAY)) AS `Year`,
                BIN_TO_UUID(`p`.`Id`) AS `ProviderId`,
                `p`.`FullTitle` AS `ProviderName`,
                `p`.`EdrpouIpn` AS `EDRPOU`,
                (CASE
                    WHEN (`p`.`Ownership` = 0) THEN 'State'
                    WHEN (`p`.`Ownership` = 1) THEN 'Common'
                    WHEN (`p`.`Ownership` = 2) THEN 'Private'
                    ELSE ''
                END) AS `ProviderType`,
                `i`.`Title` AS `InstitutionTitle`,
                `c`.`Code` AS `CATOTTGCode`,
                `c`.`Category` AS `CATOTTGCategory`,
                `c`.`Region` AS `Region`,
                `c`.`TerritorialCommunity` AS `TerritorialCommunity`,
                `c`.`Settlement` AS `Settlement`,
                (CASE
                    WHEN (`p`.`InstitutionType` = 0) THEN 'Complex'
                    WHEN (`p`.`InstitutionType` = 1) THEN 'Profile'
                    WHEN (`p`.`InstitutionType` = 2) THEN 'Specialized'
                    ELSE ''
                END) AS `Complex`,
                '' AS `Status`,
                IFNULL(`wa`.`Amount`, 0) AS `WorkshopsAmount`,
                IFNULL(`app`.`Amount`, 0) AS `ApplicationsAmount`,
                IFNULL(`app`.`Approved`, 0) AS `ApplicationsApproved`,
                IFNULL(`app`.`ChildrenStudying`, 0) AS `ChildrenStudying`,
                IFNULL(`app`.`ChildrenStudyingFemale`, 0) AS `ChildrenStudyingFemale`,
                IFNULL(`app`.`ChildrenStudyingLess18`, 0) AS `ChildrenStudyingLess18`,
                IFNULL(`t`.`Amount`, 0) AS `Teachers`
        FROM
            ((((((`outofschool`.`providers` `p`
        JOIN `outofschool`.`institutions` `i` ON ((`i`.`Id` = `p`.`InstitutionId`)))
        JOIN `outofschool`.`addresses` `a` ON ((`a`.`Id` = `p`.`LegalAddressId`)))
        JOIN (SELECT 
            `c`.`Id` AS `Id`,
                `c`.`Code` AS `Code`,
                `c`.`Name` AS `Name`,
                `c`.`Category` AS `Category`,
                (CASE
                    WHEN (`c`.`Category` = 'O') THEN `c`.`Name`
                    WHEN (`c2`.`Category` = 'O') THEN `c2`.`Name`
                    WHEN (`c3`.`Category` = 'O') THEN `c3`.`Name`
                    WHEN (`c4`.`Category` = 'O') THEN `c4`.`Name`
                    WHEN (`c5`.`Category` = 'O') THEN `c5`.`Name`
                    ELSE ''
                END) AS `Region`,
                (CASE
                    WHEN (`c`.`Category` = 'H') THEN `c`.`Name`
                    WHEN (`c2`.`Category` = 'H') THEN `c2`.`Name`
                    WHEN (`c3`.`Category` = 'H') THEN `c3`.`Name`
                    WHEN (`c4`.`Category` = 'H') THEN `c4`.`Name`
                    WHEN (`c5`.`Category` = 'H') THEN `c5`.`Name`
                    ELSE ''
                END) AS `TerritorialCommunity`,
                (CASE
                    WHEN (`c`.`Category` IN ('M' , 'T', 'C', 'X', 'K')) THEN `c`.`Name`
                    WHEN (`c2`.`Category` IN ('M' , 'T', 'C', 'X', 'K')) THEN `c2`.`Name`
                    WHEN (`c3`.`Category` IN ('M' , 'T', 'C', 'X', 'K')) THEN `c3`.`Name`
                    WHEN (`c4`.`Category` IN ('M' , 'T', 'C', 'X', 'K')) THEN `c4`.`Name`
                    WHEN (`c5`.`Category` IN ('M' , 'T', 'C', 'X', 'K')) THEN `c5`.`Name`
                    ELSE ''
                END) AS `Settlement`
        FROM
            ((((`outofschool`.`catottgs` `c`
        LEFT JOIN `outofschool`.`catottgs` `c2` ON ((`c2`.`Id` = `c`.`ParentId`)))
        LEFT JOIN `outofschool`.`catottgs` `c3` ON ((`c3`.`Id` = `c2`.`ParentId`)))
        LEFT JOIN `outofschool`.`catottgs` `c4` ON ((`c4`.`Id` = `c3`.`ParentId`)))
        LEFT JOIN `outofschool`.`catottgs` `c5` ON ((`c5`.`Id` = `c4`.`ParentId`)))) `c` ON ((`c`.`Id` = `a`.`CATOTTGId`)))
        LEFT JOIN (SELECT 
            `w`.`ProviderId` AS `ProviderId`, COUNT(0) AS `Amount`
        FROM
            `outofschool`.`workshops` `w`
        WHERE
            (0 = `w`.`IsDeleted`)
        GROUP BY `w`.`ProviderId`) `wa` ON ((`wa`.`ProviderId` = `p`.`Id`)))
        LEFT JOIN (SELECT 
            `w`.`ProviderId` AS `ProviderId`,
                COUNT(0) AS `Amount`,
                SUM((CASE
                    WHEN (`a`.`Status` IN (3 , 4)) THEN 1
                    ELSE 0
                END)) AS `Approved`,
                SUM((CASE
                    WHEN (`a`.`Status` IN (2 , 3, 4)) THEN 1
                    ELSE 0
                END)) AS `ChildrenStudying`,
                SUM((CASE
                    WHEN
                        ((`a`.`Status` IN (2 , 3, 4))
                            AND (`c`.`Gender` = 1))
                    THEN
                        1
                    ELSE 0
                END)) AS `ChildrenStudyingFemale`,
                SUM((CASE
                    WHEN
                        ((`a`.`Status` IN (2 , 3, 4))
                            AND ((CURDATE() + INTERVAL -(18) YEAR) < `c`.`DateOfBirth`))
                    THEN
                        1
                    ELSE 0
                END)) AS `ChildrenStudyingLess18`
        FROM
            ((`outofschool`.`applications` `a`
        JOIN `outofschool`.`workshops` `w` ON ((`w`.`Id` = `a`.`WorkshopId`)))
        JOIN `outofschool`.`children` `c` ON ((`c`.`Id` = `a`.`ChildId`)))
        WHERE
            ((0 = `a`.`IsDeleted`)
                AND (0 = `a`.`IsBlocked`)
                AND (0 = `w`.`IsDeleted`))
        GROUP BY `w`.`ProviderId`) `app` ON ((`app`.`ProviderId` = `p`.`Id`)))
        LEFT JOIN (SELECT 
            `w`.`ProviderId` AS `ProviderId`, COUNT(0) AS `Amount`
        FROM
            ((`outofschool`.`teachers` `t`
        JOIN `outofschool`.`workshops` `w` ON ((`w`.`Id` = `t`.`WorkshopId`)))
        JOIN `outofschool`.`providers` `p` ON ((`p`.`Id` = `w`.`ProviderId`)))
        WHERE
            ((0 = `t`.`IsDeleted`)
                AND (0 = `w`.`IsDeleted`)
                AND (0 = `p`.`IsDeleted`))
        GROUP BY `w`.`ProviderId`) `t` ON ((`t`.`ProviderId` = `p`.`Id`)))
        WHERE
            (0 = `p`.`IsDeleted`)) `firstquery`
        LEFT JOIN (SELECT 
            BIN_TO_UUID(`p`.`Id`) AS `ProviderId`,
                `wa`.`InstitutionHierarchyId` AS `InstitutionHierarchyId`,
                `ih`.`Title` AS `InstitutionHierarchyTitle`,
                `wa`.`Amount` AS `WorkshopsAmountInstitutionHierarchy`,
                IFNULL(`cih`.`ChildrenStudyingInstitutionHierarchy`, 0) AS `ChildrenStudyingInstitutionHierarchy`,
                IFNULL(`cih`.`ChildrenStudyingFemaleInstitutionHierarchy`, 0) AS `ChildrenStudyingFemaleInstitutionHierarchy`,
                IFNULL(`cih`.`ChildrenStudyingLess18`, 0) AS `ChildrenStudyingLess18`,
                IFNULL(`cih`.`ChildrenStudyingAchievementsInstitutionHierarchy`, 0) AS `ChildrenStudyingAchievementsInstitutionHierarchy`,
                IFNULL(`cih`.`ChildrenStudyingLargeFamilyInstitutionHierarchy`, 0) AS `ChildrenStudyingLargeFamilyInstitutionHierarchy`,
                IFNULL(`cih`.`ChildrenStudyingPoorFamilyInstitutionHierarchy`, 0) AS `ChildrenStudyingPoorFamilyInstitutionHierarchy`,
                IFNULL(`cih`.`ChildrenStudyingDisabilityInstitutionHierarchy`, 0) AS `ChildrenStudyingDisabilityInstitutionHierarchy`,
                IFNULL(`cih`.`ChildrenStudyingOrphanInstitutionHierarchy`, 0) AS `ChildrenStudyingOrphanInstitutionHierarchy`,
                IFNULL(`tih`.`Amount`, 0) AS `TeachersInstitutionHierarchy`,
                IFNULL(`tih`.`TeachersLess30`, 0) AS `TeachersLess30InstitutionHierarchy`,
                IFNULL(`tih`.`TeachersFrom31To40`, 0) AS `TeachersFrom31To40InstitutionHierarchy`,
                IFNULL(`tih`.`TeachersFrom41To50`, 0) AS `TeachersFrom41To50InstitutionHierarchy`,
                IFNULL(`tih`.`TeachersFrom51To55`, 0) AS `TeachersFrom51To55InstitutionHierarchy`,
                IFNULL(`tih`.`TeachersFrom55`, 0) AS `TeachersFrom55InstitutionHierarchy`
        FROM
            ((((`outofschool`.`providers` `p`
        JOIN (SELECT 
            `w`.`ProviderId` AS `ProviderId`,
                `w`.`InstitutionHierarchyId` AS `InstitutionHierarchyId`,
                COUNT(0) AS `Amount`
        FROM
            `outofschool`.`workshops` `w`
        WHERE
            (0 = `w`.`IsDeleted`)
        GROUP BY `w`.`ProviderId` , `w`.`InstitutionHierarchyId`) `wa` ON ((`wa`.`ProviderId` = `p`.`Id`)))
        LEFT JOIN `outofschool`.`institutionhierarchies` `ih` ON ((`wa`.`InstitutionHierarchyId` = `ih`.`Id`)))
        LEFT JOIN (SELECT 
            `w`.`ProviderId` AS `ProviderId`,
                `w`.`InstitutionHierarchyId` AS `InstitutionHierarchyId`,
                SUM((CASE
                    WHEN (`a`.`Status` IN (2 , 3, 4)) THEN 1
                    ELSE 0
                END)) AS `ChildrenStudyingInstitutionHierarchy`,
                SUM((CASE
                    WHEN
                        ((`a`.`Status` IN (2 , 3, 4))
                            AND (`c`.`Gender` = 1))
                    THEN
                        1
                    ELSE 0
                END)) AS `ChildrenStudyingFemaleInstitutionHierarchy`,
                SUM((CASE
                    WHEN
                        ((`a`.`Status` IN (2 , 3, 4))
                            AND ((CURDATE() + INTERVAL -(18) YEAR) < `c`.`DateOfBirth`))
                    THEN
                        1
                    ELSE 0
                END)) AS `ChildrenStudyingLess18`,
                SUM((CASE
                    WHEN
                        ((`a`.`Status` IN (2 , 3, 4))
                            AND (`ach`.`ChildrenId` IS NOT NULL))
                    THEN
                        1
                    ELSE 0
                END)) AS `ChildrenStudyingAchievementsInstitutionHierarchy`,
                SUM((CASE
                    WHEN
                        ((`a`.`Status` IN (2 , 3, 4))
                            AND (`csglargefamily`.`ChildrenId` IS NOT NULL))
                    THEN
                        1
                    ELSE 0
                END)) AS `ChildrenStudyingLargeFamilyInstitutionHierarchy`,
                SUM((CASE
                    WHEN
                        ((`a`.`Status` IN (2 , 3, 4))
                            AND (`csgpoorfamily`.`ChildrenId` IS NOT NULL))
                    THEN
                        1
                    ELSE 0
                END)) AS `ChildrenStudyingPoorFamilyInstitutionHierarchy`,
                SUM((CASE
                    WHEN
                        ((`a`.`Status` IN (2 , 3, 4))
                            AND (`csgdisability`.`ChildrenId` IS NOT NULL))
                    THEN
                        1
                    ELSE 0
                END)) AS `ChildrenStudyingDisabilityInstitutionHierarchy`,
                SUM((CASE
                    WHEN
                        ((`a`.`Status` IN (2 , 3, 4))
                            AND (`csgorphan`.`ChildrenId` IS NOT NULL))
                    THEN
                        1
                    ELSE 0
                END)) AS `ChildrenStudyingOrphanInstitutionHierarchy`
        FROM
            (((((((`outofschool`.`applications` `a`
        JOIN `outofschool`.`workshops` `w` ON ((`w`.`Id` = `a`.`WorkshopId`)))
        JOIN `outofschool`.`children` `c` ON ((`c`.`Id` = `a`.`ChildId`)))
        LEFT JOIN (SELECT DISTINCT
            `ac`.`ChildrenId` AS `ChildrenId`
        FROM
            `outofschool`.`achievementchild` `ac`) `ach` ON ((`ach`.`ChildrenId` = `c`.`Id`)))
        LEFT JOIN `outofschool`.`childsocialgroup` `csglargefamily` ON (((`csglargefamily`.`ChildrenId` = `c`.`Id`)
            AND (`csglargefamily`.`SocialGroupsId` = 1))))
        LEFT JOIN `outofschool`.`childsocialgroup` `csgpoorfamily` ON (((`csgpoorfamily`.`ChildrenId` = `c`.`Id`)
            AND (`csgpoorfamily`.`SocialGroupsId` = 2))))
        LEFT JOIN `outofschool`.`childsocialgroup` `csgdisability` ON (((`csgdisability`.`ChildrenId` = `c`.`Id`)
            AND (`csgdisability`.`SocialGroupsId` = 3))))
        LEFT JOIN `outofschool`.`childsocialgroup` `csgorphan` ON (((`csgorphan`.`ChildrenId` = `c`.`Id`)
            AND (`csgorphan`.`SocialGroupsId` IN (4 , 5)))))
        WHERE
            ((0 = `a`.`IsDeleted`)
                AND (0 = `a`.`IsBlocked`)
                AND (0 = `w`.`IsDeleted`))
        GROUP BY `w`.`ProviderId` , `w`.`InstitutionHierarchyId`) `cih` ON (((`cih`.`ProviderId` = `wa`.`ProviderId`)
            AND (`cih`.`InstitutionHierarchyId` = `wa`.`InstitutionHierarchyId`))))
        LEFT JOIN (SELECT 
            `w`.`ProviderId` AS `ProviderId`,
                `w`.`InstitutionHierarchyId` AS `InstitutionHierarchyId`,
                COUNT(0) AS `Amount`,
                SUM((CASE
                    WHEN (`ty`.`Years` < 31) THEN 1
                    ELSE 0
                END)) AS `TeachersLess30`,
                SUM((CASE
                    WHEN (`ty`.`Years` BETWEEN 31 AND 40) THEN 1
                    ELSE 0
                END)) AS `TeachersFrom31To40`,
                SUM((CASE
                    WHEN (`ty`.`Years` BETWEEN 41 AND 50) THEN 1
                    ELSE 0
                END)) AS `TeachersFrom41To50`,
                SUM((CASE
                    WHEN (`ty`.`Years` BETWEEN 51 AND 55) THEN 1
                    ELSE 0
                END)) AS `TeachersFrom51To55`,
                SUM((CASE
                    WHEN (`ty`.`Years` >= 55) THEN 1
                    ELSE 0
                END)) AS `TeachersFrom55`
        FROM
            (((`outofschool`.`teachers` `t`
        JOIN `outofschool`.`workshops` `w` ON ((`w`.`Id` = `t`.`WorkshopId`)))
        JOIN `outofschool`.`providers` `p` ON ((`p`.`Id` = `w`.`ProviderId`)))
        JOIN (SELECT 
            `t`.`Id` AS `Id`,
                TIMESTAMPDIFF(YEAR, `t`.`DateOfBirth`, CURDATE()) AS `Years`
        FROM
            `outofschool`.`teachers` `t`) `ty` ON ((`ty`.`Id` = `t`.`Id`)))
        WHERE
            ((0 = `t`.`IsDeleted`)
                AND (0 = `w`.`IsDeleted`)
                AND (0 = `p`.`IsDeleted`))
        GROUP BY `w`.`ProviderId` , `w`.`InstitutionHierarchyId`) `tih` ON (((`tih`.`ProviderId` = `wa`.`ProviderId`)
            AND (`tih`.`InstitutionHierarchyId` = `wa`.`InstitutionHierarchyId`))))
        WHERE
            (0 = `p`.`IsDeleted`)) `secondquery` ON ((`secondquery`.`ProviderId` = `firstquery`.`ProviderId`)))");

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Method intentionally left empty.
    }
}
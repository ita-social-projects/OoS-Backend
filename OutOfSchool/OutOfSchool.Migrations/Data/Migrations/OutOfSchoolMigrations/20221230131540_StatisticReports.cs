using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class StatisticReports : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "FilesInDb",
            columns: table => new
            {
                Id = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ContentType = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Data = table.Column<byte[]>(type: "longblob", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FilesInDb", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "StatisticReports",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                ReportType = table.Column<int>(type: "int", nullable: false),
                ReportDataType = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ExternalStorageId = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StatisticReports", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "StatisticReportsCSV",
            columns: table => new
            {
                Year = table.Column<int>(type: "int", nullable: false),
                ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                ProviderName = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                EDRPOU = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ProviderType = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                InstitutionTitle = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CATOTTGCode = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Region = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TerritorialCommunity = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Settlement = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CATOTTGCategory = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Complex = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Status = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                WorkshopsAmount = table.Column<int>(type: "int", nullable: false),
                ApplicationsAmount = table.Column<int>(type: "int", nullable: false),
                ApplicationsApproved = table.Column<int>(type: "int", nullable: false),
                ChildrenStudying = table.Column<int>(type: "int", nullable: false),
                ChildrenStudyingFemale = table.Column<int>(type: "int", nullable: false),
                ChildrenStudyingLess18 = table.Column<int>(type: "int", nullable: false),
                Teachers = table.Column<int>(type: "int", nullable: false),
                WorkshopsAmountInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                ChildrenStudyingInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                ChildrenStudyingFemaleInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                ChildrenStudyingAchievementsInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                ChildrenStudyingLargeFamilyInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                ChildrenStudyingPoorFamilyInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                ChildrenStudyingDisabilityInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                ChildrenStudyingOrphanInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                TeachersInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                TeachersLess30InstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                TeachersFrom31To40InstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                TeachersFrom41To50InstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                TeachersFrom51To55InstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                TeachersFrom55InstitutionHierarchy = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.Sql(@"
		DROP VIEW IF EXISTS statisticreportdata;
        CREATE VIEW `statisticreportdata` AS
		SELECT
				FirstQuery.Year,
				FirstQuery.ProviderId,
				FirstQuery.ProviderName,
				FirstQuery.EDRPOU,
				FirstQuery.ProviderType,
				FirstQuery.InstitutionTitle,
				FirstQuery.CATOTTGCode,
				FirstQuery.CATOTTGCategory,
				FirstQuery.Region,
				FirstQuery.TerritorialCommunity,
				FirstQuery.Settlement,
				FirstQuery.Complex,
				FirstQuery.Status,
				FirstQuery.WorkshopsAmount,
				FirstQuery.ApplicationsAmount,
				FirstQuery.ApplicationsApproved,
				FirstQuery.ChildrenStudying,
				FirstQuery.ChildrenStudyingFemale,
				FirstQuery.ChildrenStudyingLess18,
				FirstQuery.Teachers,
				IFNULL(SecondQuery.InstitutionHierarchyTitle, '') AS InstitutionHierarchyTitle,
				IFNULL(SecondQuery.WorkshopsAmountInstitutionHierarchy, 0) AS WorkshopsAmountInstitutionHierarchy,
				IFNULL(SecondQuery.ChildrenStudyingInstitutionHierarchy, 0) AS ChildrenStudyingInstitutionHierarchy,
				IFNULL(SecondQuery.ChildrenStudyingFemaleInstitutionHierarchy, 0) AS ChildrenStudyingFemaleInstitutionHierarchy,
				IFNULL(SecondQuery.ChildrenStudyingLess18, 0) AS ChildrenStudyingLess18InstitutionHierarchy,
				IFNULL(SecondQuery.ChildrenStudyingAchievementsInstitutionHierarchy, 0) AS ChildrenStudyingAchievementsInstitutionHierarchy,
				IFNULL(SecondQuery.ChildrenStudyingLargeFamilyInstitutionHierarchy, 0) AS ChildrenStudyingLargeFamilyInstitutionHierarchy,
				IFNULL(SecondQuery.ChildrenStudyingPoorFamilyInstitutionHierarchy, 0) AS ChildrenStudyingPoorFamilyInstitutionHierarchy,
				IFNULL(SecondQuery.ChildrenStudyingDisabilityInstitutionHierarchy, 0) AS ChildrenStudyingDisabilityInstitutionHierarchy,
				IFNULL(SecondQuery.ChildrenStudyingOrphanInstitutionHierarchy, 0) AS ChildrenStudyingOrphanInstitutionHierarchy,
				IFNULL(SecondQuery.TeachersInstitutionHierarchy, 0) AS TeachersInstitutionHierarchy,
				IFNULL(SecondQuery.TeachersLess30InstitutionHierarchy, 0) AS TeachersLess30InstitutionHierarchy,
				IFNULL(SecondQuery.TeachersFrom31To40InstitutionHierarchy, 0) AS TeachersFrom31To40InstitutionHierarchy,
				IFNULL(SecondQuery.TeachersFrom41To50InstitutionHierarchy, 0) AS TeachersFrom41To50InstitutionHierarchy,
				IFNULL(SecondQuery.TeachersFrom51To55InstitutionHierarchy, 0) AS TeachersFrom51To55InstitutionHierarchy,
				IFNULL(SecondQuery.TeachersFrom55InstitutionHierarchy, 0) AS TeachersFrom55InstitutionHierarchy
			FROM
				(SELECT
					Year(DATE_ADD(CURDATE(), INTERVAL -1 DAY)) AS Year,
					BIN_TO_UUID(p.Id) AS ProviderId,
					p.FullTitle AS ProviderName,
					p.EdrpouIpn AS EDRPOU,
					CASE 
						WHEN p.Ownership = 0 THEN 'State'
						WHEN p.Ownership = 1 THEN 'Common'
						WHEN p.Ownership = 2 THEN 'Private'
						ELSE '' 
					END AS ProviderType,
					i.Title AS InstitutionTitle,
					c.Code AS CATOTTGCode,
					c.Category AS CATOTTGCategory,
					c.Region,
					c.TerritorialCommunity,
					c.Settlement,
					CASE 
						WHEN p.InstitutionType = 0 THEN 'Complex'
						WHEN p.InstitutionType = 1 THEN 'Profile'
						WHEN p.InstitutionType = 2 THEN 'Specialized'
						ELSE '' 
					END AS Complex,
					'' AS Status,
					IFNULL(wa.Amount, 0) AS WorkshopsAmount,
					IFNULL(app.Amount, 0) AS ApplicationsAmount,
					IFNULL(app.Approved, 0) AS ApplicationsApproved,
					IFNULL(app.ChildrenStudying, 0) AS ChildrenStudying,
					IFNULL(app.ChildrenStudyingFemale, 0) AS ChildrenStudyingFemale,
					IFNULL(app.ChildrenStudyingLess18, 0) AS ChildrenStudyingLess18,
					IFNULL(t.Amount, 0) AS Teachers
				FROM
					Providers p
                    INNER JOIN Institutions i
						ON i.Id = p.InstitutionId
					INNER JOIN Addresses a
						ON a.Id = p.LegalAddressId
					INNER JOIN (SELECT
							c.Id,
							c.Code,
							c.Name,
							c.Category,
							CASE 
								WHEN c.Category = 'O' THEN c.Name
								WHEN c2.Category = 'O' THEN c2.Name
								WHEN c3.Category = 'O' THEN c3.Name
								WHEN c4.Category = 'O' THEN c4.Name
								WHEN c5.Category = 'O' THEN c5.Name
								ELSE '' 
							END AS Region,
							CASE 
								WHEN c.Category = 'H' THEN c.Name
								WHEN c2.Category = 'H' THEN c2.Name
								WHEN c3.Category = 'H' THEN c3.Name
								WHEN c4.Category = 'H' THEN c4.Name
								WHEN c5.Category = 'H' THEN c5.Name
								ELSE '' 
							END AS TerritorialCommunity,
							CASE 
								WHEN c.Category IN ('M', 'T', 'C', 'X', 'K') THEN c.Name
								WHEN c2.Category IN ('M', 'T', 'C', 'X', 'K') THEN c2.Name
								WHEN c3.Category IN ('M', 'T', 'C', 'X', 'K') THEN c3.Name
								WHEN c4.Category IN ('M', 'T', 'C', 'X', 'K') THEN c4.Name
								WHEN c5.Category IN ('M', 'T', 'C', 'X', 'K') THEN c5.Name
								ELSE '' 
							END AS Settlement
						FROM
							CATOTTGs c
							LEFT JOIN CATOTTGs c2
								ON c2.Id = c.ParentId
							LEFT JOIN CATOTTGs c3
								ON c3.Id = c2.ParentId
							LEFT JOIN CATOTTGs c4
								ON c4.Id = c3.ParentId
							LEFT JOIN CATOTTGs c5
								ON c5.Id = c4.ParentId) c
						ON c.Id = a.CATOTTGId
					LEFT JOIN (SELECT
							w.ProviderId,
							COUNT(*) AS Amount
						FROM
							Workshops w
						WHERE
							NOT w.IsDeleted
						GROUP BY w.ProviderId) wa
						ON wa.ProviderId = p.Id
					LEFT JOIN (SELECT
							w.ProviderId,
							COUNT(*) AS Amount,
							SUM(CASE WHEN a.Status IN (3,4) THEN 1 ELSE 0 END) AS Approved,
							SUM(CASE WHEN a.Status IN (2,3,4) THEN 1 ELSE 0 END) AS ChildrenStudying,
							SUM(CASE WHEN a.Status IN (2,3,4) AND c.Gender = 1 THEN 1 ELSE 0 END) AS ChildrenStudyingFemale,
							SUM(CASE WHEN a.Status IN (2,3,4) AND DATE_ADD(CURDATE(), INTERVAL -18 YEAR) < c.DateOfBirth THEN 1 ELSE 0 END) AS ChildrenStudyingLess18
						FROM
							Applications a
							INNER JOIN Workshops w
								ON w.Id = a.WorkshopId
							INNER JOIN Children c
								ON c.Id = a.ChildId
						WHERE
							NOT a.IsDeleted
							AND NOT a.IsBlocked
                            AND NOT w.IsDeleted
						GROUP BY w.ProviderId) app
						ON app.ProviderId = p.Id
					LEFT JOIN (SELECT
							w.ProviderId,
							COUNT(*) AS Amount
						FROM
							Teachers t
							INNER JOIN Workshops w
								ON w.Id = t.WorkshopId
							INNER JOIN Providers p
								ON p.Id = w.ProviderId
						WHERE
							NOT t.IsDeleted
							AND NOT w.IsDeleted
							AND NOT p.IsDeleted
						GROUP BY w.ProviderId) t
						ON t.ProviderId = p.Id
				WHERE
					NOT p.IsDeleted) AS FirstQuery
				LEFT JOIN (SELECT
							BIN_TO_UUID(p.Id) AS ProviderId,
							wa.InstitutionHierarchyId AS InstitutionHierarchyId,
							ih.Title AS InstitutionHierarchyTitle,
							wa.Amount AS WorkshopsAmountInstitutionHierarchy,
							IFNULL(cih.ChildrenStudyingInstitutionHierarchy, 0) AS ChildrenStudyingInstitutionHierarchy,
							IFNULL(cih.ChildrenStudyingFemaleInstitutionHierarchy, 0) AS ChildrenStudyingFemaleInstitutionHierarchy,
							IFNULL(cih.ChildrenStudyingLess18, 0) AS ChildrenStudyingLess18,
							IFNULL(cih.ChildrenStudyingAchievementsInstitutionHierarchy, 0) AS ChildrenStudyingAchievementsInstitutionHierarchy,
							IFNULL(cih.ChildrenStudyingLargeFamilyInstitutionHierarchy, 0) AS ChildrenStudyingLargeFamilyInstitutionHierarchy,
							IFNULL(cih.ChildrenStudyingPoorFamilyInstitutionHierarchy, 0) AS ChildrenStudyingPoorFamilyInstitutionHierarchy,
							IFNULL(cih.ChildrenStudyingDisabilityInstitutionHierarchy, 0) AS ChildrenStudyingDisabilityInstitutionHierarchy,
							IFNULL(cih.ChildrenStudyingOrphanInstitutionHierarchy, 0) AS ChildrenStudyingOrphanInstitutionHierarchy,
							IFNULL(tih.Amount, 0) AS TeachersInstitutionHierarchy,
							IFNULL(tih.TeachersLess30, 0) AS TeachersLess30InstitutionHierarchy,
							IFNULL(tih.TeachersFrom31To40, 0) AS TeachersFrom31To40InstitutionHierarchy,
							IFNULL(tih.TeachersFrom41To50, 0) AS TeachersFrom41To50InstitutionHierarchy,
							IFNULL(tih.TeachersFrom51To55, 0) AS TeachersFrom51To55InstitutionHierarchy,
							IFNULL(tih.TeachersFrom55, 0) AS TeachersFrom55InstitutionHierarchy
						FROM
							Providers p	
							INNER JOIN (SELECT
									w.ProviderId,
									w.InstitutionHierarchyId,
									COUNT(*) AS Amount
								FROM
									Workshops w
								WHERE
									NOT w.IsDeleted
								GROUP BY w.ProviderId, w.InstitutionHierarchyId) wa
								ON wa.ProviderId = p.Id
							LEFT JOIN InstitutionHierarchies ih
								ON wa.InstitutionHierarchyId = ih.Id
							LEFT JOIN (SELECT
										w.ProviderId,
										w.InstitutionHierarchyId,
										SUM(CASE WHEN a.Status IN (2,3,4) THEN 1 ELSE 0 END) AS ChildrenStudyingInstitutionHierarchy,
										SUM(CASE WHEN a.Status IN (2,3,4) AND c.Gender = 1 THEN 1 ELSE 0 END) AS ChildrenStudyingFemaleInstitutionHierarchy,
										SUM(CASE WHEN a.Status IN (2,3,4) AND DATE_ADD(CURDATE(), INTERVAL -18 YEAR) < c.DateOfBirth THEN 1 ELSE 0 END) AS ChildrenStudyingLess18,
										SUM(CASE WHEN a.Status IN (2,3,4) AND ach.ChildrenId IS NOT NULL THEN 1 ELSE 0 END) AS ChildrenStudyingAchievementsInstitutionHierarchy,
										SUM(CASE WHEN a.Status IN (2,3,4) AND csgLargeFamily.ChildrenId IS NOT NULL THEN 1 ELSE 0 END) AS ChildrenStudyingLargeFamilyInstitutionHierarchy,
										SUM(CASE WHEN a.Status IN (2,3,4) AND csgPoorFamily.ChildrenId IS NOT NULL THEN 1 ELSE 0 END) AS ChildrenStudyingPoorFamilyInstitutionHierarchy,
										SUM(CASE WHEN a.Status IN (2,3,4) AND csgDisability.ChildrenId IS NOT NULL THEN 1 ELSE 0 END) AS ChildrenStudyingDisabilityInstitutionHierarchy,
										SUM(CASE WHEN a.Status IN (2,3,4) AND csgOrphan.ChildrenId IS NOT NULL THEN 1 ELSE 0 END) AS ChildrenStudyingOrphanInstitutionHierarchy
									FROM
										Applications a
										INNER JOIN Workshops w
											ON w.Id = a.WorkshopId
										INNER JOIN Children c
											ON c.Id = a.ChildId
										LEFT JOIN (SELECT DISTINCT
												ac.ChildrenId
											FROM 
												AchievementChild ac) ach
											ON ach.ChildrenId = c.Id
										LEFT JOIN ChildSocialGroup csgLargeFamily
											ON csgLargeFamily.ChildrenId = c.Id AND csgLargeFamily.SocialGroupsId = 1
										LEFT JOIN ChildSocialGroup csgPoorFamily
											ON csgPoorFamily.ChildrenId = c.Id AND csgPoorFamily.SocialGroupsId = 2
										LEFT JOIN ChildSocialGroup csgDisability
											ON csgDisability.ChildrenId = c.Id AND csgDisability.SocialGroupsId = 3
										LEFT JOIN ChildSocialGroup csgOrphan
											ON csgOrphan.ChildrenId = c.Id AND csgOrphan.SocialGroupsId IN (4,5)
									WHERE
										NOT a.IsDeleted
										AND NOT a.IsBlocked
                                        AND NOT w.IsDeleted
									GROUP BY w.ProviderId, w.InstitutionHierarchyId) cih
								ON cih.ProviderId = wa.ProviderId AND cih.InstitutionHierarchyId = wa.InstitutionHierarchyId
							LEFT JOIN (SELECT
										w.ProviderId,
										w.InstitutionHierarchyId,
										COUNT(*) AS Amount,
										SUM(CASE WHEN ty.Years < 31 THEN 1 ELSE 0 END) AS TeachersLess30,
										SUM(CASE WHEN ty.Years BETWEEN 31 AND 40 THEN 1 ELSE 0 END) AS TeachersFrom31To40,
										SUM(CASE WHEN ty.Years BETWEEN 41 AND 50 THEN 1 ELSE 0 END) AS TeachersFrom41To50,
										SUM(CASE WHEN ty.Years BETWEEN 51 AND 55 THEN 1 ELSE 0 END) AS TeachersFrom51To55,
										SUM(CASE WHEN ty.Years >= 55 THEN 1 ELSE 0 END) AS TeachersFrom55
									FROM
										Teachers t
										INNER JOIN Workshops w
											ON w.Id = t.WorkshopId
										INNER JOIN Providers p
											ON p.Id = w.ProviderId
										INNER JOIN (SELECT t.Id, TIMESTAMPDIFF(YEAR, t.DateOfBirth, CURDATE()) AS Years FROM Teachers t) ty
											ON ty.Id = t.Id
									WHERE
										NOT t.IsDeleted
										AND NOT w.IsDeleted
										AND NOT p.IsDeleted
									GROUP BY w.ProviderId, w.InstitutionHierarchyId) tih
								ON tih.ProviderId = wa.ProviderId AND tih.InstitutionHierarchyId = wa.InstitutionHierarchyId
						WHERE
							NOT p.IsDeleted) AS SecondQuery
					ON SecondQuery.ProviderId = FirstQuery.ProviderId;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
		migrationBuilder.Sql(@"
		DROP VIEW IF EXISTS statisticreportdata;");
		
        migrationBuilder.DropTable(
            name: "FilesInDb");

        migrationBuilder.DropTable(
            name: "StatisticReports");

        migrationBuilder.DropTable(
            name: "StatisticReportsCSV");
    }
}

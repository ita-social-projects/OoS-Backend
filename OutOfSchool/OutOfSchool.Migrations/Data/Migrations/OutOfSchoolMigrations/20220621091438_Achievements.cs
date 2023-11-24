using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class Achievements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                name: "AchievementTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTypes", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AchievementDate = table.Column<DateTime>(type: "date", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    AchievementTypeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Achievements_AchievementTypes_AchievementTypeId",
                        column: x => x.AchievementTypeId,
                        principalTable: "AchievementTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Achievements_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AchievementChild",
                columns: table => new
                {
                    AchievementsId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ChildrenId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementChild", x => new { x.AchievementsId, x.ChildrenId });
                    table.ForeignKey(
                        name: "FK_AchievementChild_Achievements_AchievementsId",
                        column: x => x.AchievementsId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AchievementChild_Children_ChildrenId",
                        column: x => x.ChildrenId,
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AchievementTeachers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AchievementId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTeachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AchievementTeachers_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.InsertData(
            table: "AchievementTypes",
            columns: new[] { "Id", "Title" },
            values: new object[,]
            {
                { 1L, "Переможці міжнародних та всеукраїнських спортивних змагань (індивідуальних та командних)" },
                { 2L, "Призери та учасники міжнародних, всеукраїнських та призери регіональних конкурсів і виставок наукових, технічних, дослідницьких, інноваційних, ІТ проектів" },
                { 3L, "Реципієнти міжнародних грантів" },
                { 4L, "Призери міжнародних культурних конкурсів та фестивалів" },
                { 5L, "Соціально активні категорії учнів" },
                { 6L, "Цифрові інструменти Google для закладів вищої та фахової передвищої освіти" },
                { 7L, "Переможці та учасники олімпіад міжнародного та всеукраїнського рівнів" }
            });

        migrationBuilder.CreateIndex(
            name: "IX_AchievementChild_ChildrenId",
            table: "AchievementChild",
            column: "ChildrenId");

        migrationBuilder.CreateIndex(
            name: "IX_Achievements_AchievementTypeId",
            table: "Achievements",
            column: "AchievementTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_Achievements_WorkshopId",
            table: "Achievements",
            column: "WorkshopId");

        migrationBuilder.CreateIndex(
            name: "IX_AchievementTeachers_AchievementId",
            table: "AchievementTeachers",
            column: "AchievementId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AchievementChild");

        migrationBuilder.DropTable(
            name: "AchievementTeachers");

        migrationBuilder.DropTable(
            name: "Achievements");

        migrationBuilder.DropTable(
            name: "AchievementTypes");
    }
}
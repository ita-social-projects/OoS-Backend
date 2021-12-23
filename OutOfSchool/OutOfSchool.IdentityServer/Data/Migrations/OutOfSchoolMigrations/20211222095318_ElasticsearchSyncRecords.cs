using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class ElasticsearchSyncRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElasticsearchSyncRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Entity = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    OperationDate = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Operation = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElasticsearchSyncRecords", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElasticsearchSyncRecords");
        }
    }
}

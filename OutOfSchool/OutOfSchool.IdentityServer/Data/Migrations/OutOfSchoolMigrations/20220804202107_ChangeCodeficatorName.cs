using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class ChangeCodeficatorName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Codeficators_CodeficatorId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Addresses_ActualAddressId",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Addresses_LegalAddressId",
                table: "Providers");

            migrationBuilder.DropTable(
                name: "Codeficators");

            migrationBuilder.DropIndex(
                name: "IX_Providers_ActualAddressId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_LegalAddressId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_CodeficatorId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "CodeficatorId",
                table: "Addresses");

            migrationBuilder.AddColumn<long>(
                name: "CATOTTGId",
                table: "Addresses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CATOTTGs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Category = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    GeoHash = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    NeedCheck = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATOTTGs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CATOTTGs_CATOTTGs_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CATOTTGs",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_ActualAddressId",
                table: "Providers",
                column: "ActualAddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Providers_LegalAddressId",
                table: "Providers",
                column: "LegalAddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CATOTTGId",
                table: "Addresses",
                column: "CATOTTGId");

            migrationBuilder.CreateIndex(
                name: "IX_CATOTTGs_ParentId",
                table: "CATOTTGs",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_CATOTTGs_CATOTTGId",
                table: "Addresses",
                column: "CATOTTGId",
                principalTable: "CATOTTGs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Addresses_ActualAddressId",
                table: "Providers",
                column: "ActualAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Addresses_LegalAddressId",
                table: "Providers",
                column: "LegalAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_CATOTTGs_CATOTTGId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Addresses_ActualAddressId",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Addresses_LegalAddressId",
                table: "Providers");

            migrationBuilder.DropTable(
                name: "CATOTTGs");

            migrationBuilder.DropIndex(
                name: "IX_Providers_ActualAddressId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_LegalAddressId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_CATOTTGId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "CATOTTGId",
                table: "Addresses");

            migrationBuilder.AddColumn<long>(
                name: "CodeficatorId",
                table: "Addresses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Codeficators",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Category = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeoHash = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NeedCheck = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Codeficators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Codeficators_Codeficators_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Codeficators",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_ActualAddressId",
                table: "Providers",
                column: "ActualAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_LegalAddressId",
                table: "Providers",
                column: "LegalAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CodeficatorId",
                table: "Addresses",
                column: "CodeficatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Codeficators_ParentId",
                table: "Codeficators",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Codeficators_CodeficatorId",
                table: "Addresses",
                column: "CodeficatorId",
                principalTable: "Codeficators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Addresses_ActualAddressId",
                table: "Providers",
                column: "ActualAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Addresses_LegalAddressId",
                table: "Providers",
                column: "LegalAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

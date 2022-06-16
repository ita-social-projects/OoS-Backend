using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class ChangesLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangesLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EntityType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PropertyName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityIdGuid = table.Column<Guid>(type: "binary(16)", nullable: true),
                    EntityIdLong = table.Column<long>(type: "bigint", nullable: true),
                    OldValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangesLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangesLog_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProviderAdminChangesLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProviderAdminUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ManagedWorkshopId = table.Column<Guid>(type: "binary(16)", nullable: true),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    OperationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAdminChangesLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderAdminChangesLog_AspNetUsers_ProviderAdminUserId",
                        column: x => x.ProviderAdminUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderAdminChangesLog_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderAdminChangesLog_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderAdminChangesLog_Workshops_ManagedWorkshopId",
                        column: x => x.ManagedWorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChangesLog_UserId",
                table: "ChangesLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAdminChangesLog_ManagedWorkshopId",
                table: "ProviderAdminChangesLog",
                column: "ManagedWorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAdminChangesLog_ProviderAdminUserId",
                table: "ProviderAdminChangesLog",
                column: "ProviderAdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAdminChangesLog_ProviderId",
                table: "ProviderAdminChangesLog",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAdminChangesLog_UserId",
                table: "ProviderAdminChangesLog",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangesLog");

            migrationBuilder.DropTable(
                name: "ProviderAdminChangesLog");
        }
    }
}

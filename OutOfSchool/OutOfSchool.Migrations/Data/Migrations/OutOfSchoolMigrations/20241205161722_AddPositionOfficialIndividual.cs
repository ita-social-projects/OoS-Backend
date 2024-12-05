using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddPositionOfficialIndividual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderAdminChangesLog");

            migrationBuilder.DropTable(
                name: "ProviderAdminWorkshop");

            migrationBuilder.DropTable(
                name: "ProviderAdmins");

            migrationBuilder.CreateTable(
                name: "EmployeeChangesLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    OperationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PropertyName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeChangesLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeChangesLog_AspNetUsers_EmployeeUserId",
                        column: x => x.EmployeeUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeChangesLog_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeChangesLog_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    BlockingType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Employees_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Individuals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    IsRegistered = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rnokpp = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalRegistryId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Document = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    File = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ActiveTo = table.Column<DateOnly>(type: "date", nullable: false, defaultValue: new DateOnly(9999, 12, 31)),
                    IsBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSystemProtected = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Individuals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Individuals_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Language = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsForRuralAreas = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Department = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ContactId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    SeatsAmount = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GenitiveName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsTeachingPosition = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Rate = table.Column<float>(type: "float", nullable: false),
                    Tariff = table.Column<float>(type: "float", nullable: false),
                    ClassifierType = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Document = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    File = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ActiveTo = table.Column<DateOnly>(type: "date", nullable: false),
                    IsBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSystemProtected = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Positions_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmployeeWorkshop",
                columns: table => new
                {
                    EmployeesUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManagedWorkshopsId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeWorkshop", x => new { x.EmployeesUserId, x.ManagedWorkshopsId });
                    table.ForeignKey(
                        name: "FK_EmployeeWorkshop_Employees_EmployeesUserId",
                        column: x => x.EmployeesUserId,
                        principalTable: "Employees",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkshop_Workshops_ManagedWorkshopsId",
                        column: x => x.ManagedWorkshopsId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Officials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    PositionId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    IndividualId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    DismissalOrder = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecruitmentOrder = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DismissalReason = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmploymentType = table.Column<int>(type: "int", nullable: false),
                    ExternalRegistryId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Document = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    File = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ActiveTo = table.Column<DateOnly>(type: "date", nullable: false),
                    IsBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSystemProtected = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Officials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Officials_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Officials_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "Description", "RoleName" },
                values: new object[] { "employee permissions", "Employee" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeChangesLog_EmployeeUserId",
                table: "EmployeeChangesLog",
                column: "EmployeeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeChangesLog_ProviderId",
                table: "EmployeeChangesLog",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeChangesLog_UserId",
                table: "EmployeeChangesLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsDeleted",
                table: "Employees",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ProviderId",
                table: "Employees",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkshop_ManagedWorkshopsId",
                table: "EmployeeWorkshop",
                column: "ManagedWorkshopsId");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_IsDeleted",
                table: "Individuals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_Rnokpp",
                table: "Individuals",
                column: "Rnokpp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_UserId",
                table: "Individuals",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Officials_IndividualId",
                table: "Officials",
                column: "IndividualId");

            migrationBuilder.CreateIndex(
                name: "IX_Officials_PositionId",
                table: "Officials",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ProviderId",
                table: "Positions",
                column: "ProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeChangesLog");

            migrationBuilder.DropTable(
                name: "EmployeeWorkshop");

            migrationBuilder.DropTable(
                name: "Officials");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Individuals");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.CreateTable(
                name: "ProviderAdminChangesLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProviderAdminUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeputy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NewValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    PropertyName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProviderAdmins",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    BlockingType = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsDeputy = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAdmins", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ProviderAdmins_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProviderAdminWorkshop",
                columns: table => new
                {
                    ManagedWorkshopsId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ProviderAdminsUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAdminWorkshop", x => new { x.ManagedWorkshopsId, x.ProviderAdminsUserId });
                    table.ForeignKey(
                        name: "FK_ProviderAdminWorkshop_ProviderAdmins_ProviderAdminsUserId",
                        column: x => x.ProviderAdminsUserId,
                        principalTable: "ProviderAdmins",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderAdminWorkshop_Workshops_ManagedWorkshopsId",
                        column: x => x.ManagedWorkshopsId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "Description", "RoleName" },
                values: new object[] { "provider admin permissions", "ProviderAdmin" });

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

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAdmins_IsDeleted",
                table: "ProviderAdmins",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAdmins_ProviderId",
                table: "ProviderAdmins",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAdminWorkshop_ProviderAdminsUserId",
                table: "ProviderAdminWorkshop",
                column: "ProviderAdminsUserId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class UpdatedProviderAndWorkshop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Addresses_AddressId",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Addresses_AddressId",
                table: "Workshops");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Providers_ProviderId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "AttachedDocuments",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "AuthorityHolder",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "DirectorPhone",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "DirectorPosition",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Form",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "KOATUU",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Profile",
                table: "Providers");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Workshops",
                newName: "Logo");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Providers",
                newName: "FullTitle");

            migrationBuilder.RenameColumn(
                name: "ManagerialBody",
                table: "Providers",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "MFO",
                table: "Providers",
                newName: "Founder");

            migrationBuilder.RenameColumn(
                name: "IsSubmitPZ1",
                table: "Providers",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "INPP",
                table: "Providers",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "EDRPOU",
                table: "Providers",
                newName: "EdrpouIpn");

            migrationBuilder.RenameColumn(
                name: "AddressId",
                table: "Providers",
                newName: "LegalAddressId");

            migrationBuilder.RenameIndex(
                name: "IX_Providers_AddressId",
                table: "Providers",
                newName: "IX_Providers_LegalAddressId");

            migrationBuilder.AlterColumn<long>(
                name: "ProviderId",
                table: "Workshops",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "AddressId",
                table: "Workshops",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPerMonth",
                table: "Workshops",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ActualAddressId",
                table: "Providers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);          

            migrationBuilder.CreateIndex(
                name: "IX_Providers_ActualAddressId",
                table: "Providers",
                column: "ActualAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Addresses_ActualAddressId",
                table: "Providers",
                column: "ActualAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Addresses_LegalAddressId",
                table: "Providers",
                column: "LegalAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Addresses_AddressId",
                table: "Workshops",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Providers_ProviderId",
                table: "Workshops",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);          
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Addresses_ActualAddressId",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Addresses_LegalAddressId",
                table: "Providers");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Addresses_AddressId",
                table: "Workshops");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Providers_ProviderId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Providers_ActualAddressId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "IsPerMonth",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "ActualAddressId",
                table: "Providers");
          
            migrationBuilder.RenameColumn(
                name: "Logo",
                table: "Workshops",
                newName: "Image");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Providers",
                newName: "IsSubmitPZ1");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Providers",
                newName: "ManagerialBody");

            migrationBuilder.RenameColumn(
                name: "LegalAddressId",
                table: "Providers",
                newName: "AddressId");

            migrationBuilder.RenameColumn(
                name: "FullTitle",
                table: "Providers",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Founder",
                table: "Providers",
                newName: "MFO");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Providers",
                newName: "INPP");

            migrationBuilder.RenameColumn(
                name: "EdrpouIpn",
                table: "Providers",
                newName: "EDRPOU");

            migrationBuilder.RenameIndex(
                name: "IX_Providers_LegalAddressId",
                table: "Providers",
                newName: "IX_Providers_AddressId");

            migrationBuilder.AlterColumn<long>(
                name: "ProviderId",
                table: "Workshops",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "AddressId",
                table: "Workshops",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "AttachedDocuments",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorityHolder",
                table: "Providers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DirectorPhone",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DirectorPosition",
                table: "Providers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Form",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Index",
                table: "Providers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KOATUU",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Profile",
                table: "Providers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Addresses_AddressId",
                table: "Providers",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Addresses_AddressId",
                table: "Workshops",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Providers_ProviderId",
                table: "Workshops",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

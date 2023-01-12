using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddCodeficatorParentsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CodeficatorParents",
            columns: table => new
            {
                CatottgsId = table.Column<long>(type: "bigint", nullable: false),
                ParentId = table.Column<long>(type: "bigint", nullable: false),
                Level = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.ForeignKey(
                    name: "FK_CodeficatorParents_CATOTTGs_CatottgsId",
                    column: x => x.CatottgsId,
                    principalTable: "CATOTTGs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CodeficatorParents_CATOTTGs_ParentId",
                    column: x => x.ParentId,
                    principalTable: "CATOTTGs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_CodeficatorParents_CatottgsId",
            table: "CodeficatorParents",
            column: "CatottgsId");

        migrationBuilder.CreateIndex(
            name: "IX_CodeficatorParents_ParentId",
            table: "CodeficatorParents",
            column: "ParentId");

        migrationBuilder.Sql(@"
        DROP TRIGGER IF EXISTS catottgs_AFTER_DELETE;

        CREATE DEFINER = CURRENT_USER TRIGGER catottgs_AFTER_DELETE AFTER DELETE ON catottgs FOR EACH ROW
        BEGIN
	        DELETE FROM codeficatorparents WHERE catottgsId = old.Id;	
	        DELETE FROM codeficatorparents WHERE parentId = old.Id;
        END;");

        migrationBuilder.Sql(@"
	    DROP TRIGGER IF EXISTS catottgs_AFTER_INSERT;

        CREATE DEFINER = CURRENT_USER TRIGGER catottgs_AFTER_INSERT AFTER INSERT ON catottgs FOR EACH ROW
        BEGIN
	        INSERT INTO codeficatorparents (catottgsId, parentId, level)(
	        with recursive parent_users (id, parentid, level) AS (
	          SELECT new.id, new.parentid, 1 level
	          union all
	          SELECT t.id, t.parentid, level + 1
	          FROM catottgs t INNER JOIN parent_users pu
	          ON t.id = pu.parentid
	        )
	        SELECT new.id, parentid, level FROM parent_users WHERE parentid IS NOT NULL);
        END;");

        migrationBuilder.Sql(@"
	    DROP TRIGGER IF EXISTS catottgs_AFTER_UPDATE;

        CREATE DEFINER = CURRENT_USER TRIGGER catottgs_AFTER_UPDATE AFTER UPDATE ON catottgs FOR EACH ROW
        BEGIN
	        IF (new.parentId <> old.parentId OR new.id <> old.id) THEN
		        DELETE 
	                c1 
                FROM 
	                codeficatorparents c1 
                    INNER JOIN  (SELECT catottgsid FROM codeficatorparents WHERE parentid = old.id
			
			                UNION
			
			                SELECT new.id
            
			                UNION
            
			                SELECT old.id) c2 
		                ON c2.catottgsId = c1.catottgsId;
        
                INSERT INTO codeficatorparents (catottgsId, parentId, level)(
		        with recursive parent_users (id, parentid, level) AS (
		          SELECT id, parentid, 1 level
                  FROM catottgs t
                  WHERE id IN (SELECT catottgsid From codeficatorparents WHERE parentid = old.id
			
			            UNION
			
			            SELECT new.id
            
			            UNION
            
			            SELECT old.id)
		          union all
		          SELECT pu.id, t.parentid, level + 1
		          FROM catottgs t INNER JOIN parent_users pu
		          ON t.id = pu.parentid
		        )
		        SELECT id, parentid, level FROM parent_users WHERE parentid IS NOT NULL);        
	        END IF;
        END;");

        migrationBuilder.Sql(@"
	    DELETE FROM codeficatorparents;

        INSERT INTO codeficatorparents (catottgsId, parentId, level)(
        with recursive parent_users (id, parentid, level) AS (
          SELECT id, parentid, 1 level
          FROM catottgs t
          union all
          SELECT pu.id, t.parentid, level + 1
          FROM catottgs t INNER JOIN parent_users pu
          ON t.id = pu.parentid
        )
        SELECT id, parentid, level FROM parent_users WHERE parentid IS NOT NULL);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
	    DROP TRIGGER IF EXISTS catottgs_AFTER_UPDATE;

        DROP TRIGGER IF EXISTS catottgs_AFTER_INSERT;

        DROP TRIGGER IF EXISTS catottgs_AFTER_DELETE;");

        migrationBuilder.DropTable(
            name: "CodeficatorParents");
    }
}

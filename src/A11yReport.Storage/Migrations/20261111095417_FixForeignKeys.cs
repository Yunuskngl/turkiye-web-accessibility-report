using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace A11yReport.Storage.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep existing data: copy links from the hidden convention columns into our own
            // columns before they are dropped, and remove rows whose parent was already deleted.
            migrationBuilder.Sql("UPDATE Sessions SET RunId = ScanRunEntityId WHERE ScanRunEntityId IS NOT NULL;");
            migrationBuilder.Sql("UPDATE Sites SET RunId = ScanRunEntityId WHERE ScanRunEntityId IS NOT NULL;");
            migrationBuilder.Sql("UPDATE Violations SET SiteScanId = SiteScanEntityId WHERE SiteScanEntityId IS NOT NULL;");
            migrationBuilder.Sql("UPDATE ViolationNodeEntity SET ViolationId = ViolationEntityId WHERE ViolationEntityId IS NOT NULL;");
            migrationBuilder.Sql("DELETE FROM ViolationNodeEntity WHERE ViolationEntityId IS NULL;");
            migrationBuilder.Sql("DELETE FROM Violations WHERE SiteScanEntityId IS NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Runs_ScanRunEntityId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Runs_ScanRunEntityId",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_ViolationNodeEntity_Violations_ViolationEntityId",
                table: "ViolationNodeEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_Violations_Sites_SiteScanEntityId",
                table: "Violations");

            migrationBuilder.DropIndex(
                name: "IX_Violations_SiteScanEntityId",
                table: "Violations");

            migrationBuilder.DropIndex(
                name: "IX_Sites_ScanRunEntityId",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_ScanRunEntityId",
                table: "Sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ViolationNodeEntity",
                table: "ViolationNodeEntity");

            migrationBuilder.DropIndex(
                name: "IX_ViolationNodeEntity_ViolationEntityId",
                table: "ViolationNodeEntity");

            migrationBuilder.DropColumn(
                name: "SiteScanEntityId",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "ScanRunEntityId",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "ScanRunEntityId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ViolationEntityId",
                table: "ViolationNodeEntity");

            migrationBuilder.RenameTable(
                name: "ViolationNodeEntity",
                newName: "ViolationNodes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ViolationNodes",
                table: "ViolationNodes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_SiteScanId",
                table: "Violations",
                column: "SiteScanId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RunId",
                table: "Sessions",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationNodes_ViolationId",
                table: "ViolationNodes",
                column: "ViolationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Runs_RunId",
                table: "Sessions",
                column: "RunId",
                principalTable: "Runs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Runs_RunId",
                table: "Sites",
                column: "RunId",
                principalTable: "Runs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ViolationNodes_Violations_ViolationId",
                table: "ViolationNodes",
                column: "ViolationId",
                principalTable: "Violations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Violations_Sites_SiteScanId",
                table: "Violations",
                column: "SiteScanId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Runs_RunId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Runs_RunId",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_ViolationNodes_Violations_ViolationId",
                table: "ViolationNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Violations_Sites_SiteScanId",
                table: "Violations");

            migrationBuilder.DropIndex(
                name: "IX_Violations_SiteScanId",
                table: "Violations");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_RunId",
                table: "Sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ViolationNodes",
                table: "ViolationNodes");

            migrationBuilder.DropIndex(
                name: "IX_ViolationNodes_ViolationId",
                table: "ViolationNodes");

            migrationBuilder.RenameTable(
                name: "ViolationNodes",
                newName: "ViolationNodeEntity");

            migrationBuilder.AddColumn<int>(
                name: "SiteScanEntityId",
                table: "Violations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScanRunEntityId",
                table: "Sites",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScanRunEntityId",
                table: "Sessions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViolationEntityId",
                table: "ViolationNodeEntity",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ViolationNodeEntity",
                table: "ViolationNodeEntity",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_SiteScanEntityId",
                table: "Violations",
                column: "SiteScanEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_ScanRunEntityId",
                table: "Sites",
                column: "ScanRunEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ScanRunEntityId",
                table: "Sessions",
                column: "ScanRunEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationNodeEntity_ViolationEntityId",
                table: "ViolationNodeEntity",
                column: "ViolationEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Runs_ScanRunEntityId",
                table: "Sessions",
                column: "ScanRunEntityId",
                principalTable: "Runs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Runs_ScanRunEntityId",
                table: "Sites",
                column: "ScanRunEntityId",
                principalTable: "Runs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ViolationNodeEntity_Violations_ViolationEntityId",
                table: "ViolationNodeEntity",
                column: "ViolationEntityId",
                principalTable: "Violations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Violations_Sites_SiteScanEntityId",
                table: "Violations",
                column: "SiteScanEntityId",
                principalTable: "Sites",
                principalColumn: "Id");
        }
    }
}

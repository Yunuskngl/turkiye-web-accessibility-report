using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace A11yReport.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddLoadEventReached : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LoadEventReached",
                table: "Sites",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoadEventReached",
                table: "Sites");
        }
    }
}

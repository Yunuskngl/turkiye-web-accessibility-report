using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace A11yReport.Storage.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Runs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ListSource = table.Column<string>(type: "TEXT", nullable: false),
                    ListSha256 = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    State = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RunId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    ScannerVersion = table.Column<string>(type: "TEXT", nullable: false),
                    DotnetVersion = table.Column<string>(type: "TEXT", nullable: false),
                    PlaywrightVersion = table.Column<string>(type: "TEXT", nullable: false),
                    BrowserVersion = table.Column<string>(type: "TEXT", nullable: false),
                    AxeVersion = table.Column<string>(type: "TEXT", nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: false),
                    SettingsJson = table.Column<string>(type: "TEXT", nullable: false),
                    ScanRunEntityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Runs_ScanRunEntityId",
                        column: x => x.ScanRunEntityId,
                        principalTable: "Runs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RunId = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: false),
                    Sector = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    HttpStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    FinalUrl = table.Column<string>(type: "TEXT", nullable: true),
                    FinalSiteKey = table.Column<string>(type: "TEXT", nullable: true),
                    AxeVersion = table.Column<string>(type: "TEXT", nullable: true),
                    RobotsRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    DocumentRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    SubresourceRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    Lang = table.Column<string>(type: "TEXT", nullable: true),
                    HasTitle = table.Column<bool>(type: "INTEGER", nullable: true),
                    LandmarkCount = table.Column<int>(type: "INTEGER", nullable: true),
                    HasMain = table.Column<bool>(type: "INTEGER", nullable: true),
                    HeadingCount = table.Column<int>(type: "INTEGER", nullable: true),
                    H1Count = table.Column<int>(type: "INTEGER", nullable: true),
                    SkipsHeadingLevel = table.Column<bool>(type: "INTEGER", nullable: true),
                    ScanRunEntityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sites_Runs_ScanRunEntityId",
                        column: x => x.ScanRunEntityId,
                        principalTable: "Runs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Violations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteScanId = table.Column<int>(type: "INTEGER", nullable: false),
                    Viewport = table.Column<string>(type: "TEXT", nullable: false),
                    RuleId = table.Column<string>(type: "TEXT", nullable: false),
                    Impact = table.Column<string>(type: "TEXT", nullable: false),
                    Help = table.Column<string>(type: "TEXT", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    NodeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SiteScanEntityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Violations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Violations_Sites_SiteScanEntityId",
                        column: x => x.SiteScanEntityId,
                        principalTable: "Sites",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ViolationNodeEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ViolationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Selector = table.Column<string>(type: "TEXT", nullable: false),
                    Snippet = table.Column<string>(type: "TEXT", nullable: false),
                    ViolationEntityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationNodeEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViolationNodeEntity_Violations_ViolationEntityId",
                        column: x => x.ViolationEntityId,
                        principalTable: "Violations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Runs_Name",
                table: "Runs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ScanRunEntityId",
                table: "Sessions",
                column: "ScanRunEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_RunId_Domain",
                table: "Sites",
                columns: new[] { "RunId", "Domain" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_ScanRunEntityId",
                table: "Sites",
                column: "ScanRunEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationNodeEntity_ViolationEntityId",
                table: "ViolationNodeEntity",
                column: "ViolationEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_RuleId",
                table: "Violations",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_SiteScanEntityId",
                table: "Violations",
                column: "SiteScanEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "ViolationNodeEntity");

            migrationBuilder.DropTable(
                name: "Violations");

            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.DropTable(
                name: "Runs");
        }
    }
}

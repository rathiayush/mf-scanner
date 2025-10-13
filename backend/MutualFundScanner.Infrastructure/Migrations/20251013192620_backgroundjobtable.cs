using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MutualFundScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class backgroundjobtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioHoldings_PortfolioSnapshots_PortfolioSnapshotId",
                table: "PortfolioHoldings");

            migrationBuilder.CreateTable(
                name: "BackgroundJobs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DirectoryPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformDateCheck = table.Column<bool>(type: "bit", nullable: false),
                    FilePatterns = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    GenerateExcelReport = table.Column<bool>(type: "bit", nullable: false),
                    TotalFiles = table.Column<int>(type: "int", nullable: false),
                    ProcessedFiles = table.Column<int>(type: "int", nullable: false),
                    SuccessfulFiles = table.Column<int>(type: "int", nullable: false),
                    FailedFiles = table.Column<int>(type: "int", nullable: false),
                    CurrentFile = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExcelReportPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_CreatedAt",
                table: "BackgroundJobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_Status",
                table: "BackgroundJobs",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioHoldings_PortfolioSnapshots_PortfolioSnapshotId",
                table: "PortfolioHoldings",
                column: "PortfolioSnapshotId",
                principalTable: "PortfolioSnapshots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioHoldings_PortfolioSnapshots_PortfolioSnapshotId",
                table: "PortfolioHoldings");

            migrationBuilder.DropTable(
                name: "BackgroundJobs");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioHoldings_PortfolioSnapshots_PortfolioSnapshotId",
                table: "PortfolioHoldings",
                column: "PortfolioSnapshotId",
                principalTable: "PortfolioSnapshots",
                principalColumn: "Id");
        }
    }
}

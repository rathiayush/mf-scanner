using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MutualFundScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MutualFunds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FundCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FundName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FolderPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastScanDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MutualFunds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoldingChanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MutualFundId = table.Column<int>(type: "int", nullable: false),
                    ISIN = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Ticker = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecurityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    PreviousQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    NewQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PreviousWeight = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    NewWeight = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NewValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoldingChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoldingChanges_MutualFunds_MutualFundId",
                        column: x => x.MutualFundId,
                        principalTable: "MutualFunds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PortfolioFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MutualFundId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordCount = table.Column<int>(type: "int", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioFiles_MutualFunds_MutualFundId",
                        column: x => x.MutualFundId,
                        principalTable: "MutualFunds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PortfolioSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MutualFundId = table.Column<int>(type: "int", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalHoldings = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioSnapshots_MutualFunds_MutualFundId",
                        column: x => x.MutualFundId,
                        principalTable: "MutualFunds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProcessingLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioFileId = table.Column<int>(type: "int", nullable: false),
                    LogLevel = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessingLogs_PortfolioFiles_PortfolioFileId",
                        column: x => x.PortfolioFileId,
                        principalTable: "PortfolioFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PortfolioHoldings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioFileId = table.Column<int>(type: "int", nullable: false),
                    PortfolioSnapshotId = table.Column<int>(type: "int", nullable: true),
                    ISIN = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Ticker = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecurityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Sector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MarketValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    PreviousWeight = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    AsOfDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioHoldings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioHoldings_PortfolioFiles_PortfolioFileId",
                        column: x => x.PortfolioFileId,
                        principalTable: "PortfolioFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PortfolioHoldings_PortfolioSnapshots_PortfolioSnapshotId",
                        column: x => x.PortfolioSnapshotId,
                        principalTable: "PortfolioSnapshots",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoldingChanges_MutualFundId_ISIN_FromDate_ToDate",
                table: "HoldingChanges",
                columns: new[] { "MutualFundId", "ISIN", "FromDate", "ToDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MutualFunds_FundCode",
                table: "MutualFunds",
                column: "FundCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioFiles_FileHash",
                table: "PortfolioFiles",
                column: "FileHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioFiles_MutualFundId",
                table: "PortfolioFiles",
                column: "MutualFundId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioHoldings_PortfolioFileId_ISIN_AsOfDate",
                table: "PortfolioHoldings",
                columns: new[] { "PortfolioFileId", "ISIN", "AsOfDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioHoldings_PortfolioSnapshotId",
                table: "PortfolioHoldings",
                column: "PortfolioSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSnapshots_MutualFundId_SnapshotDate",
                table: "PortfolioSnapshots",
                columns: new[] { "MutualFundId", "SnapshotDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingLogs_PortfolioFileId",
                table: "ProcessingLogs",
                column: "PortfolioFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoldingChanges");

            migrationBuilder.DropTable(
                name: "PortfolioHoldings");

            migrationBuilder.DropTable(
                name: "ProcessingLogs");

            migrationBuilder.DropTable(
                name: "PortfolioSnapshots");

            migrationBuilder.DropTable(
                name: "PortfolioFiles");

            migrationBuilder.DropTable(
                name: "MutualFunds");
        }
    }
}

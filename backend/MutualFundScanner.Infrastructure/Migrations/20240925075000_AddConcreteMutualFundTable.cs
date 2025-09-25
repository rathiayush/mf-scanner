using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MutualFundScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConcreteMutualFundTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConcreteMutualFunds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FundName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AMCName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MatchPriority = table.Column<int>(type: "int", nullable: false, defaultValue: 100)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcreteMutualFunds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConcreteMutualFunds_FundName",
                table: "ConcreteMutualFunds",
                column: "FundName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConcreteMutualFunds_AMCName",
                table: "ConcreteMutualFunds",
                column: "AMCName");

            migrationBuilder.CreateIndex(
                name: "IX_ConcreteMutualFunds_Category",
                table: "ConcreteMutualFunds",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ConcreteMutualFunds_MatchPriority",
                table: "ConcreteMutualFunds",
                column: "MatchPriority");

            migrationBuilder.CreateIndex(
                name: "IX_ConcreteMutualFunds_IsActive_MatchPriority",
                table: "ConcreteMutualFunds",
                columns: new[] { "IsActive", "MatchPriority" });

            // Seed initial data
            migrationBuilder.Sql(@"
                INSERT INTO ConcreteMutualFunds (Id, FundName, AMCName, Category, SubCategory, MatchPriority, IsActive, CreatedDate, LastUpdated, Notes)
                VALUES
                (1, 'Axis Bluechip Fund', 'Axis Mutual Fund', 'Equity', 'Large Cap', 1, 1, GETUTCDATE(), GETUTCDATE(), 'Large cap equity fund'),
                (2, 'HDFC Top 100 Fund', 'HDFC Mutual Fund', 'Equity', 'Large Cap', 1, 1, GETUTCDATE(), GETUTCDATE(), 'Large cap equity fund'),
                (3, 'ICICI Prudential Bluechip Fund', 'ICICI Prudential Mutual Fund', 'Equity', 'Large Cap', 1, 1, GETUTCDATE(), GETUTCDATE(), 'Large cap equity fund'),
                (4, 'SBI Bluechip Fund', 'SBI Mutual Fund', 'Equity', 'Large Cap', 1, 1, GETUTCDATE(), GETUTCDATE(), 'Large cap equity fund'),
                (5, 'Mirae Asset Large Cap Fund', 'Mirae Asset Mutual Fund', 'Equity', 'Large Cap', 1, 1, GETUTCDATE(), GETUTCDATE(), 'Large cap equity fund'),
                (6, 'Axis Midcap Fund', 'Axis Mutual Fund', 'Equity', 'Mid Cap', 2, 1, GETUTCDATE(), GETUTCDATE(), 'Mid cap equity fund'),
                (7, 'HDFC Mid-Cap Opportunities Fund', 'HDFC Mutual Fund', 'Equity', 'Mid Cap', 2, 1, GETUTCDATE(), GETUTCDATE(), 'Mid cap equity fund'),
                (8, 'ICICI Prudential Midcap Fund', 'ICICI Prudential Mutual Fund', 'Equity', 'Mid Cap', 2, 1, GETUTCDATE(), GETUTCDATE(), 'Mid cap equity fund'),
                (9, 'SBI Magnum Midcap Fund', 'SBI Mutual Fund', 'Equity', 'Mid Cap', 2, 1, GETUTCDATE(), GETUTCDATE(), 'Mid cap equity fund'),
                (10, 'Axis Small Cap Fund', 'Axis Mutual Fund', 'Equity', 'Small Cap', 3, 1, GETUTCDATE(), GETUTCDATE(), 'Small cap equity fund'),
                (11, 'HDFC Small Cap Fund', 'HDFC Mutual Fund', 'Equity', 'Small Cap', 3, 1, GETUTCDATE(), GETUTCDATE(), 'Small cap equity fund'),
                (12, 'ICICI Prudential Smallcap Fund', 'ICICI Prudential Mutual Fund', 'Equity', 'Small Cap', 3, 1, GETUTCDATE(), GETUTCDATE(), 'Small cap equity fund'),
                (13, 'SBI Small Cap Fund', 'SBI Mutual Fund', 'Equity', 'Small Cap', 3, 1, GETUTCDATE(), GETUTCDATE(), 'Small cap equity fund'),
                (14, 'Axis Liquid Fund', 'Axis Mutual Fund', 'Debt', 'Liquid', 4, 1, GETUTCDATE(), GETUTCDATE(), 'Liquid debt fund'),
                (15, 'HDFC Liquid Fund', 'HDFC Mutual Fund', 'Debt', 'Liquid', 4, 1, GETUTCDATE(), GETUTCDATE(), 'Liquid debt fund'),
                (16, 'ICICI Prudential Liquid Fund', 'ICICI Prudential Mutual Fund', 'Debt', 'Liquid', 4, 1, GETUTCDATE(), GETUTCDATE(), 'Liquid debt fund'),
                (17, 'SBI Liquid Fund', 'SBI Mutual Fund', 'Debt', 'Liquid', 4, 1, GETUTCDATE(), GETUTCDATE(), 'Liquid debt fund'),
                (18, 'Axis Arbitrage Fund', 'Axis Mutual Fund', 'Hybrid', 'Arbitrage', 5, 1, GETUTCDATE(), GETUTCDATE(), 'Arbitrage hybrid fund'),
                (19, 'HDFC Arbitrage Fund', 'HDFC Mutual Fund', 'Hybrid', 'Arbitrage', 5, 1, GETUTCDATE(), GETUTCDATE(), 'Arbitrage hybrid fund'),
                (20, 'ICICI Prudential Equity Arbitrage Fund', 'ICICI Prudential Mutual Fund', 'Hybrid', 'Arbitrage', 5, 1, GETUTCDATE(), GETUTCDATE(), 'Arbitrage hybrid fund');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConcreteMutualFunds");
        }
    }
}

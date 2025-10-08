using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MutualFundScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedmutualfundIdtoportfolioholding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MutualFundId",
                table: "PortfolioHoldings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioHoldings_MutualFundId",
                table: "PortfolioHoldings",
                column: "MutualFundId");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioHoldings_MutualFunds_MutualFundId",
                table: "PortfolioHoldings",
                column: "MutualFundId",
                principalTable: "MutualFunds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioHoldings_MutualFunds_MutualFundId",
                table: "PortfolioHoldings");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioHoldings_MutualFundId",
                table: "PortfolioHoldings");

            migrationBuilder.DropColumn(
                name: "MutualFundId",
                table: "PortfolioHoldings");
        }
    }
}

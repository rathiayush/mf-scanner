using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MutualFundScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addmutualfund : Migration
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
                    AMCName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MatchPriority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcreteMutualFunds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConcreteMutualFunds_AMCName_Category",
                table: "ConcreteMutualFunds",
                columns: new[] { "AMCName", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_ConcreteMutualFunds_FundName",
                table: "ConcreteMutualFunds",
                column: "FundName");

            migrationBuilder.CreateIndex(
                name: "IX_ConcreteMutualFunds_MatchPriority",
                table: "ConcreteMutualFunds",
                column: "MatchPriority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConcreteMutualFunds");
        }
    }
}

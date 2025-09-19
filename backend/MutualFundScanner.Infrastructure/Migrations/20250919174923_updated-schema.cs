using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MutualFundScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AMC",
                table: "MutualFunds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "AUM",
                table: "MutualFunds",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "MutualFunds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExpenseRatio",
                table: "MutualFunds",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "MutualFunds",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LaunchDate",
                table: "MutualFunds",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "NAV",
                table: "MutualFunds",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MutualFunds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "MutualFunds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AMC",
                table: "MutualFunds");

            migrationBuilder.DropColumn(
                name: "AUM",
                table: "MutualFunds");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "MutualFunds");

            migrationBuilder.DropColumn(
                name: "ExpenseRatio",
                table: "MutualFunds");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "MutualFunds");

            migrationBuilder.DropColumn(
                name: "LaunchDate",
                table: "MutualFunds");

            migrationBuilder.DropColumn(
                name: "NAV",
                table: "MutualFunds");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MutualFunds");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "MutualFunds");
        }
    }
}

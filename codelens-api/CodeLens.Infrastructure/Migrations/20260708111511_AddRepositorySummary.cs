using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRepositorySummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Repositories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SummaryGeneratedAt",
                table: "Repositories",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "SummaryGeneratedAt",
                table: "Repositories");
        }
    }
}

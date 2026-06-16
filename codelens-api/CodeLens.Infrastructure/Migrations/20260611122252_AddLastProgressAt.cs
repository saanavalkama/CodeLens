using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLastProgressAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastProgressAt",
                table: "Repositories",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastProgressAt",
                table: "Repositories");
        }
    }
}

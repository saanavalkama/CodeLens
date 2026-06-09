using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexingProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IndexedFiles",
                table: "Repositories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalFiles",
                table: "Repositories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndexedFiles",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "TotalFiles",
                table: "Repositories");
        }
    }
}

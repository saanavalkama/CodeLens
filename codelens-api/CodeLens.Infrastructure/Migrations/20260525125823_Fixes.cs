using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTruncated",
                table: "Repositories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RepositoryFiles_RepositoryId_Path",
                table: "RepositoryFiles",
                columns: new[] { "RepositoryId", "Path" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepositoryFiles_RepositoryId_Path",
                table: "RepositoryFiles");

            migrationBuilder.DropColumn(
                name: "IsTruncated",
                table: "Repositories");
        }
    }
}

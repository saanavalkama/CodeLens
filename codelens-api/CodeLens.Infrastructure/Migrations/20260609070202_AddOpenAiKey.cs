using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenAiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedOpenAIKey",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedOpenAIKey",
                table: "Users");
        }
    }
}

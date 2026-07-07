using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileChunkSearchVector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GraphEdges_GraphNodes_SouceId",
                table: "GraphEdges");

            migrationBuilder.RenameColumn(
                name: "SouceId",
                table: "GraphEdges",
                newName: "SourceId");

            migrationBuilder.RenameIndex(
                name: "IX_GraphEdges_SouceId",
                table: "GraphEdges",
                newName: "IX_GraphEdges_SourceId");

            migrationBuilder.RenameIndex(
                name: "IX_GraphEdges_RepositoryId_SouceId",
                table: "GraphEdges",
                newName: "IX_GraphEdges_RepositoryId_SourceId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FileContents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_GraphEdges_GraphNodes_SourceId",
                table: "GraphEdges",
                column: "SourceId",
                principalTable: "GraphNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(@"
                ALTER TABLE ""FileChunks"" ADD COLUMN search_vector tsvector
                GENERATED ALWAYS AS (to_tsvector('simple', ""Content"")) STORED;
                CREATE INDEX idx_filechunks_search ON ""FileChunks"" USING GIN (search_vector);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GraphEdges_GraphNodes_SourceId",
                table: "GraphEdges");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FileContents");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "GraphEdges",
                newName: "SouceId");

            migrationBuilder.RenameIndex(
                name: "IX_GraphEdges_SourceId",
                table: "GraphEdges",
                newName: "IX_GraphEdges_SouceId");

            migrationBuilder.RenameIndex(
                name: "IX_GraphEdges_RepositoryId_SourceId",
                table: "GraphEdges",
                newName: "IX_GraphEdges_RepositoryId_SouceId");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphEdges_GraphNodes_SouceId",
                table: "GraphEdges",
                column: "SouceId",
                principalTable: "GraphNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_filechunks_search;
                ALTER TABLE ""FileChunks"" DROP COLUMN search_vector;
            ");
        }
    }
}

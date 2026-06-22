using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGraphAndFileContents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RepositoryFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileContents_RepositoryFiles_RepositoryFileId",
                        column: x => x.RepositoryFileId,
                        principalTable: "RepositoryFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GraphNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RepositoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Signature = table.Column<string>(type: "text", nullable: true),
                    StartLine = table.Column<int>(type: "integer", nullable: true),
                    EndLine = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GraphNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GraphNodes_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GraphEdges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RepositoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SouceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EdgeType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GraphEdges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GraphEdges_GraphNodes_SouceId",
                        column: x => x.SouceId,
                        principalTable: "GraphNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GraphEdges_GraphNodes_TargetId",
                        column: x => x.TargetId,
                        principalTable: "GraphNodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GraphEdges_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileContents_RepositoryFileId",
                table: "FileContents",
                column: "RepositoryFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GraphEdges_RepositoryId_SouceId",
                table: "GraphEdges",
                columns: new[] { "RepositoryId", "SouceId" });

            migrationBuilder.CreateIndex(
                name: "IX_GraphEdges_RepositoryId_TargetId",
                table: "GraphEdges",
                columns: new[] { "RepositoryId", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_GraphEdges_SouceId",
                table: "GraphEdges",
                column: "SouceId");

            migrationBuilder.CreateIndex(
                name: "IX_GraphEdges_TargetId",
                table: "GraphEdges",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_GraphNodes_FilePath",
                table: "GraphNodes",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_GraphNodes_RepositoryId",
                table: "GraphNodes",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GraphNodes_RepositoryId_Name",
                table: "GraphNodes",
                columns: new[] { "RepositoryId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileContents");

            migrationBuilder.DropTable(
                name: "GraphEdges");

            migrationBuilder.DropTable(
                name: "GraphNodes");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostMediaEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "media_public_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "media_type",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "media_url",
                table: "posts");

            migrationBuilder.CreateTable(
                name: "post_media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    public_id = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    media_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    width = table.Column<int>(type: "int", nullable: true),
                    height = table.Column<int>(type: "int", nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    format = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    duration_seconds = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_post_media_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_post_media_post_id",
                table: "post_media",
                column: "post_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_media");

            migrationBuilder.AddColumn<string>(
                name: "media_public_id",
                table: "posts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "media_type",
                table: "posts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "media_url",
                table: "posts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}

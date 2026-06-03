using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHashtags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hashtags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hashtags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "post_hashtags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hashtag_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_hashtags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_post_hashtags_hashtags_hashtag_id",
                        column: x => x.hashtag_id,
                        principalTable: "hashtags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_post_hashtags_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hashtags_name",
                table: "hashtags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_hashtags_hashtag_id",
                table: "post_hashtags",
                column: "hashtag_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_hashtags_post_hashtag",
                table: "post_hashtags",
                columns: new[] { "post_id", "hashtag_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_hashtags");

            migrationBuilder.DropTable(
                name: "hashtags");
        }
    }
}

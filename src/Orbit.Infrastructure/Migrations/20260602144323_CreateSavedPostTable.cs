using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSavedPostTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "save_count",
                table: "posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "saved_posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saved_posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_saved_posts_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_saved_posts_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_saved_posts_post_id",
                table: "saved_posts",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ux_saved_posts_profile_post",
                table: "saved_posts",
                columns: new[] { "profile_id", "post_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "saved_posts");

            migrationBuilder.DropColumn(
                name: "save_count",
                table: "posts");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    actor_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    comment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    post_preview = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    comment_preview = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    total_count = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_comments_comment_id",
                        column: x => x.comment_id,
                        principalTable: "comments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_notifications_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_notifications_profiles_actor_profile_id",
                        column: x => x.actor_profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_notifications_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_actor_profile_id",
                table: "notifications",
                column: "actor_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_comment_id",
                table: "notifications",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_post_id",
                table: "notifications",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_profile_read_created",
                table: "notifications",
                columns: new[] { "profile_id", "is_read", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}

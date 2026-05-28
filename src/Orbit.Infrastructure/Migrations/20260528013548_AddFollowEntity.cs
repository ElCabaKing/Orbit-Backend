using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "follows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    follower_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    following_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_follows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_follows_profiles_follower_id",
                        column: x => x.follower_id,
                        principalTable: "profiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_follows_profiles_following_id",
                        column: x => x.following_id,
                        principalTable: "profiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_follows_follower",
                table: "follows",
                column: "follower_id");

            migrationBuilder.CreateIndex(
                name: "ix_follows_following",
                table: "follows",
                column: "following_id");

            migrationBuilder.CreateIndex(
                name: "ux_follows_follower_following",
                table: "follows",
                columns: new[] { "follower_id", "following_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "follows");
        }
    }
}

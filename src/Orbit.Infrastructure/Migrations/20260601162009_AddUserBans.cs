using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_bans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    blocker_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    blocked_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_bans", x => x.Id);
                    table.CheckConstraint("chk_user_bans_self", "blocker_profile_id <> blocked_profile_id");
                    table.ForeignKey(
                        name: "FK_user_bans_profiles_blocked_profile_id",
                        column: x => x.blocked_profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_user_bans_profiles_blocker_profile_id",
                        column: x => x.blocker_profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_bans_blocked",
                table: "user_bans",
                column: "blocked_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_bans_blocker",
                table: "user_bans",
                column: "blocker_profile_id");

            migrationBuilder.CreateIndex(
                name: "ux_user_bans_blocker_blocked",
                table: "user_bans",
                columns: new[] { "blocker_profile_id", "blocked_profile_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_bans");
        }
    }
}

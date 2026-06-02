using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateCommunityPhase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "community_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    community_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    invited_by_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    responded_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_community_invitations_communities_community_id",
                        column: x => x.community_id,
                        principalTable: "communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_community_invitations_profiles_invited_by_profile_id",
                        column: x => x.invited_by_profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_community_invitations_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "community_join_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    community_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    responded_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_join_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_community_join_requests_communities_community_id",
                        column: x => x.community_id,
                        principalTable: "communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_community_join_requests_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_community_invitations_invited_by_profile_id",
                table: "community_invitations",
                column: "invited_by_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_community_invitations_profile_id",
                table: "community_invitations",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "ux_community_invitations_community_profile",
                table: "community_invitations",
                columns: new[] { "community_id", "profile_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_community_join_requests_profile_id",
                table: "community_join_requests",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "ux_community_join_requests_community_profile",
                table: "community_join_requests",
                columns: new[] { "community_id", "profile_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "community_invitations");

            migrationBuilder.DropTable(
                name: "community_join_requests");
        }
    }
}

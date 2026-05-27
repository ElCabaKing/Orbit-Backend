using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auth_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_email_verified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_prefixes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    icon_url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_prefixes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    auth_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    refresh_token_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_sessions_auth_users_auth_user_id",
                        column: x => x.auth_user_id,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    auth_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    username_slug = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    profile_picture_url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    banner_url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    prefix_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    pinned_post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    followers_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    following_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    posts_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_verified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_premium = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_private = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_profiles_auth_users_auth_user_id",
                        column: x => x.auth_user_id,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_profiles_user_prefixes_prefix_id",
                        column: x => x.prefix_id,
                        principalTable: "user_prefixes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.profile_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_auth_users_email",
                table: "auth_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_profiles_prefix_id",
                table: "profiles",
                column: "prefix_id");

            migrationBuilder.CreateIndex(
                name: "ux_profiles_auth_user",
                table: "profiles",
                column: "auth_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_profiles_username",
                table: "profiles",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_profiles_username_slug",
                table: "profiles",
                column: "username_slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_user_prefixes_name",
                table: "user_prefixes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_auth_user",
                table: "user_sessions",
                column: "auth_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "auth_users");

            migrationBuilder.DropTable(
                name: "user_prefixes");
        }
    }
}

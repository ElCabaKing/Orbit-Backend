using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndBans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "user_roles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTime>(
                name: "banned_at",
                table: "profiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "banned_by_profile_id",
                table: "profiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_banned",
                table: "profiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles",
                column: "id");

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "created_at", "name" },
                values: new object[,]
                {
                    { new Guid("00000001-0000-0000-0000-000000000001"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin" },
                    { new Guid("00000001-0000-0000-0000-000000000002"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "moderator" },
                    { new Guid("00000001-0000-0000-0000-000000000003"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "user" }
                });

            migrationBuilder.CreateIndex(
                name: "ux_user_roles_profile_role",
                table: "user_roles",
                columns: new[] { "profile_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_profiles_banned_by_profile_id",
                table: "profiles",
                column: "banned_by_profile_id");

            migrationBuilder.AddForeignKey(
                name: "FK_profiles_profiles_banned_by_profile_id",
                table: "profiles",
                column: "banned_by_profile_id",
                principalTable: "profiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_profiles_profiles_banned_by_profile_id",
                table: "profiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "ux_user_roles_profile_role",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "IX_profiles_banned_by_profile_id",
                table: "profiles");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("00000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("00000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("00000001-0000-0000-0000-000000000003"));

            migrationBuilder.DropColumn(
                name: "id",
                table: "user_roles");

            migrationBuilder.DropColumn(
                name: "banned_at",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "banned_by_profile_id",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "is_banned",
                table: "profiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles",
                columns: new[] { "profile_id", "role_id" });
        }
    }
}

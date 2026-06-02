using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenKeyToUserSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.user_sessions', 'token_key') IS NOT NULL
                    ALTER TABLE [user_sessions] DROP COLUMN [token_key]
                """);

            migrationBuilder.AddColumn<string>(
                name: "token_key",
                table: "user_sessions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_token_key",
                table: "user_sessions",
                column: "token_key",
                unique: true,
                filter: "[token_key] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_sessions_token_key",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "token_key",
                table: "user_sessions");
        }
    }
}

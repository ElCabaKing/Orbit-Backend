using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssignInitialAdmin : Migration
    {
        private const string AdminRoleId = "00000001-0000-0000-0000-000000000001";
        private const string TargetProfileId = "47522CC2-2790-440A-B308-1E833290A03D";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                IF EXISTS (
                    SELECT 1 FROM profiles WHERE Id = '{TargetProfileId}'
                )
                AND NOT EXISTS (
                    SELECT 1 FROM user_roles
                    WHERE profile_id = '{TargetProfileId}' AND role_id = '{AdminRoleId}'
                )
                    INSERT INTO user_roles (id, profile_id, role_id, assigned_at)
                    VALUES (NEWID(), '{TargetProfileId}', '{AdminRoleId}', SYSUTCDATETIME());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DELETE FROM user_roles
                WHERE profile_id = '{TargetProfileId}' AND role_id = '{AdminRoleId}';
            ");
        }
    }
}

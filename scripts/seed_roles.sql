/*
====================================================================
 ORBIT SOCIAL
 ROLES SEED & ADMIN ASSIGNMENT
 Run after migrations have been applied.
====================================================================
*/

-- Insert roles if they don't exist
IF NOT EXISTS (SELECT 1 FROM roles WHERE name = 'admin')
    INSERT INTO roles (Id, name, created_at)
    VALUES ('00000001-0000-0000-0000-000000000001', 'admin', SYSUTCDATETIME());
GO

IF NOT EXISTS (SELECT 1 FROM roles WHERE name = 'moderator')
    INSERT INTO roles (Id, name, created_at)
    VALUES ('00000001-0000-0000-0000-000000000002', 'moderator', SYSUTCDATETIME());
GO

IF NOT EXISTS (SELECT 1 FROM roles WHERE name = 'user')
    INSERT INTO roles (Id, name, created_at)
    VALUES ('00000001-0000-0000-0000-000000000003', 'user', SYSUTCDATETIME());
GO

-- Assign admin role to the specified profile
DECLARE @adminRoleId UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000001';
DECLARE @targetProfileId UNIQUEIDENTIFIER = '47522CC2-2790-440A-B308-1E833290A03D';

IF NOT EXISTS (
    SELECT 1 FROM user_roles
    WHERE profile_id = @targetProfileId AND role_id = @adminRoleId
)
    INSERT INTO user_roles ( profile_id, role_id, assigned_at)
    VALUES ('61B015CC-082F-4437-8690-1C9E74BE1B9A','00000001-0000-0000-0000-000000000001', SYSUTCDATETIME());
GO



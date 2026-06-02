/*
====================================================================
 ORBIT SOCIAL
 ASSIGN "user" ROLE TO ALL EXISTING USERS WITHOUT IT
 Run this after migrations to ensure all existing profiles
 have the default "user" role assigned.
====================================================================
*/
DECLARE @userRoleId UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000003';

INSERT INTO user_roles (id, profile_id, role_id, assigned_at)
SELECT NEWID(), p.Id, @userRoleId, SYSUTCDATETIME()
FROM profiles p
WHERE NOT EXISTS (
    SELECT 1 FROM user_roles ur
    WHERE ur.profile_id = p.Id AND ur.role_id = @userRoleId
);
GO

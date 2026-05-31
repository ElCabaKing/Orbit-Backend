/*
====================================================================
 ORBIT SOCIAL
 EMAIL TEMPLATES SEED
 Placeholders are replaced at runtime by the application.
====================================================================
*/

/*
  Template: welcome
  Placeholders: {{displayName}}, {{username}}
*/
INSERT INTO email_templates (id, name, subject, html_body, is_active, created_at, updated_at)
VALUES (
    NEWID(),
    'welcome',
    'Welcome to Orbit, {{displayName}}!',
    '<html><body style="margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,sans-serif;">
    <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f4f4;padding:40px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.1);">
                    <tr>
                        <td style="background:linear-gradient(135deg,#6C63FF,#3F3D9E);padding:40px 30px;text-align:center;">
                            <h1 style="color:#ffffff;margin:0;font-size:28px;">Welcome to Orbit!</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:40px 30px;">
                            <p style="font-size:16px;color:#333333;line-height:1.6;">Hi <strong>{{displayName}}</strong>,</p>
                            <p style="font-size:16px;color:#333333;line-height:1.6;">Welcome to <strong>Orbit</strong>! We''re thrilled to have you on board.</p>
                            <p style="font-size:16px;color:#333333;line-height:1.6;">Your username is: <strong>{{username}}</strong></p>
                            <p style="font-size:16px;color:#333333;line-height:1.6;">Start exploring, connect with people, and share your thoughts with the world.</p>
                            <table cellpadding="0" cellspacing="0" style="margin:30px 0;">
                                <tr>
                                    <td align="center" style="background:linear-gradient(135deg,#6C63FF,#3F3D9E);border-radius:8px;padding:12px 32px;">
                                        <a href="{{frontendUrl}}/login" style="color:#ffffff;text-decoration:none;font-size:16px;font-weight:bold;">Get Started</a>
                                    </td>
                                </tr>
                            </table>
                            <p style="font-size:14px;color:#999999;line-height:1.6;">If you didn''t create this account, please ignore this email.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8f8f8;padding:20px 30px;text-align:center;border-top:1px solid #eeeeee;">
                            <p style="font-size:12px;color:#999999;margin:0;">&copy; 2025 Orbit Social. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body></html>',
    1,
    SYSUTCDATETIME(),
    SYSUTCDATETIME()
);
GO

/*
  Template: password-reset
  Placeholders: {{displayName}}, {{username}}, {{token}}, {{resetUrl}}
*/
INSERT INTO email_templates (id, name, subject, html_body, is_active, created_at, updated_at)
VALUES (
    NEWID(),
    'password-reset',
    'Orbit - Password Reset',
    '<html><body style="margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,sans-serif;">
    <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f4f4;padding:40px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.1);">
                    <tr>
                        <td style="background:linear-gradient(135deg,#6C63FF,#3F3D9E);padding:40px 30px;text-align:center;">
                            <h1 style="color:#ffffff;margin:0;font-size:28px;">Password Reset</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:40px 30px;">
                            <p style="font-size:16px;color:#333333;line-height:1.6;">Hi <strong>{{displayName}}</strong>,</p>
                            <p style="font-size:16px;color:#333333;line-height:1.6;">We received a request to reset your password. Use the link below to proceed:</p>
                            <table cellpadding="0" cellspacing="0" style="margin:30px 0;">
                                <tr>
                                    <td align="center" style="background:linear-gradient(135deg,#6C63FF,#3F3D9E);border-radius:8px;padding:12px 32px;">
                                        <a href="{{resetUrl}}" style="color:#ffffff;text-decoration:none;font-size:16px;font-weight:bold;">Reset Password</a>
                                    </td>
                                </tr>
                            </table>
                            <p style="font-size:16px;color:#333333;line-height:1.6;">Your username: <strong>{{username}}</strong></p>
                            <p style="font-size:16px;color:#333333;line-height:1.6;">This link will expire in <strong>15 minutes</strong>.</p>
                            <p style="font-size:14px;color:#999999;line-height:1.6;">If you did not request this, please ignore this email.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8f8f8;padding:20px 30px;text-align:center;border-top:1px solid #eeeeee;">
                            <p style="font-size:12px;color:#999999;margin:0;">&copy; 2025 Orbit Social. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body></html>',
    1,
    SYSUTCDATETIME(),
    SYSUTCDATETIME()
);
GO

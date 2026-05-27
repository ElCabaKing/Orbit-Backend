/*
====================================================================
 ORBIT SOCIAL
 FINAL DATABASE SCHEMA
 SQL SERVER + EF CORE CODE FIRST REFERENCE
====================================================================
*/

CREATE DATABASE OrbitSocial;
GO

USE OrbitSocial;
GO

/*
====================================================================
 AUTH USERS
====================================================================
*/

CREATE TABLE auth_users (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    email NVARCHAR(255) NOT NULL,
    password_hash NVARCHAR(500) NOT NULL,

    is_email_verified BIT NOT NULL DEFAULT 0,
    is_active BIT NOT NULL DEFAULT 1,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

GO

CREATE UNIQUE INDEX ux_auth_users_email
ON auth_users(email);

GO

/*
====================================================================
 USER PREFIXES
====================================================================
*/

CREATE TABLE user_prefixes (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    name NVARCHAR(50) NOT NULL,
    color NVARCHAR(20) NULL,
    icon_url NVARCHAR(1000) NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

GO

CREATE UNIQUE INDEX ux_user_prefixes_name
ON user_prefixes(name);

GO

/*
====================================================================
 ROLES
====================================================================
*/

CREATE TABLE roles (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    name NVARCHAR(50) NOT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

GO

CREATE UNIQUE INDEX ux_roles_name
ON roles(name);

GO

/*
====================================================================
 PROFILES
====================================================================
*/

CREATE TABLE profiles (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    auth_user_id UNIQUEIDENTIFIER NOT NULL,

    username NVARCHAR(30) NOT NULL,
    username_slug NVARCHAR(30) NOT NULL,

    display_name NVARCHAR(100) NOT NULL,

    bio NVARCHAR(500) NULL,

    profile_picture_url NVARCHAR(1000) NULL,
    banner_url NVARCHAR(1000) NULL,

    prefix_id UNIQUEIDENTIFIER NULL,

    pinned_post_id UNIQUEIDENTIFIER NULL,

    followers_count INT NOT NULL DEFAULT 0,
    following_count INT NOT NULL DEFAULT 0,
    posts_count INT NOT NULL DEFAULT 0,

    is_verified BIT NOT NULL DEFAULT 0,
    is_premium BIT NOT NULL DEFAULT 0,
    is_private BIT NOT NULL DEFAULT 0,
    is_active BIT NOT NULL DEFAULT 1,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_profiles_auth_user
        FOREIGN KEY (auth_user_id)
        REFERENCES auth_users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_profiles_prefix
        FOREIGN KEY (prefix_id)
        REFERENCES user_prefixes(id)
);

GO

CREATE UNIQUE INDEX ux_profiles_auth_user
ON profiles(auth_user_id);

CREATE UNIQUE INDEX ux_profiles_username
ON profiles(username);

CREATE UNIQUE INDEX ux_profiles_username_slug
ON profiles(username_slug);

GO

/*
====================================================================
 USER ROLES
====================================================================
*/

CREATE TABLE user_roles (
    profile_id UNIQUEIDENTIFIER NOT NULL,
    role_id UNIQUEIDENTIFIER NOT NULL,

    assigned_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_user_roles
        PRIMARY KEY (profile_id, role_id),

    CONSTRAINT fk_user_roles_profile
        FOREIGN KEY (profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_user_roles_role
        FOREIGN KEY (role_id)
        REFERENCES roles(id)
        ON DELETE CASCADE
);

GO

/*
====================================================================
 FOLLOWS
====================================================================
*/

CREATE TABLE follows (
    follower_profile_id UNIQUEIDENTIFIER NOT NULL,
    following_profile_id UNIQUEIDENTIFIER NOT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_follows
        PRIMARY KEY (follower_profile_id, following_profile_id),

    CONSTRAINT fk_follows_follower
        FOREIGN KEY (follower_profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_follows_following
        FOREIGN KEY (following_profile_id)
        REFERENCES profiles(id),

    CONSTRAINT chk_follows_self
        CHECK (follower_profile_id <> following_profile_id)
);

GO

CREATE INDEX ix_follows_following
ON follows(following_profile_id);

GO

/*
====================================================================
 USER BANS
====================================================================
*/

CREATE TABLE user_bans (
    blocker_profile_id UNIQUEIDENTIFIER NOT NULL,
    blocked_profile_id UNIQUEIDENTIFIER NOT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_user_bans
        PRIMARY KEY (blocker_profile_id, blocked_profile_id),

    CONSTRAINT fk_user_bans_blocker
        FOREIGN KEY (blocker_profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_user_bans_blocked
        FOREIGN KEY (blocked_profile_id)
        REFERENCES profiles(id),

    CONSTRAINT chk_user_bans_self
        CHECK (blocker_profile_id <> blocked_profile_id)
);

GO

/*
====================================================================
 MUTED USERS
====================================================================
*/

CREATE TABLE muted_users (
    muter_profile_id UNIQUEIDENTIFIER NOT NULL,
    muted_profile_id UNIQUEIDENTIFIER NOT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_muted_users
        PRIMARY KEY (muter_profile_id, muted_profile_id),

    CONSTRAINT fk_muted_users_muter
        FOREIGN KEY (muter_profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_muted_users_muted
        FOREIGN KEY (muted_profile_id)
        REFERENCES profiles(id),

    CONSTRAINT chk_muted_users_self
        CHECK (muter_profile_id <> muted_profile_id)
);

GO

/*
====================================================================
 COMMUNITIES
====================================================================
*/

CREATE TABLE communities (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    owner_profile_id UNIQUEIDENTIFIER NOT NULL,

    name NVARCHAR(100) NOT NULL,
    slug NVARCHAR(100) NOT NULL,

    description NVARCHAR(1000) NULL,

    member_count INT NOT NULL DEFAULT 0,

    is_private BIT NOT NULL DEFAULT 0,

    banner_url NVARCHAR(1000) NULL,
    icon_url NVARCHAR(1000) NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    deleted_at DATETIME2 NULL,

    CONSTRAINT fk_communities_owner
        FOREIGN KEY (owner_profile_id)
        REFERENCES profiles(id)
);

GO

CREATE UNIQUE INDEX ux_communities_slug
ON communities(slug);

GO

/*
====================================================================
 COMMUNITY MEMBERS
====================================================================
*/

CREATE TABLE community_members (
    community_id UNIQUEIDENTIFIER NOT NULL,
    profile_id UNIQUEIDENTIFIER NOT NULL,

    role NVARCHAR(20) NOT NULL DEFAULT 'member',

    joined_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_community_members
        PRIMARY KEY (community_id, profile_id),

    CONSTRAINT fk_community_members_community
        FOREIGN KEY (community_id)
        REFERENCES communities(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_community_members_profile
        FOREIGN KEY (profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_community_member_role
        CHECK (role IN ('owner', 'co_leader', 'member'))
);

GO

/*
====================================================================
 POSTS
====================================================================
*/

CREATE TABLE posts (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    profile_id UNIQUEIDENTIFIER NOT NULL,

    community_id UNIQUEIDENTIFIER NULL,

    parent_post_id UNIQUEIDENTIFIER NULL,
    quoted_post_id UNIQUEIDENTIFIER NULL,

    content NVARCHAR(1000) NOT NULL,

    visibility NVARCHAR(20) NOT NULL DEFAULT 'public',

    like_count INT NOT NULL DEFAULT 0,
    repost_count INT NOT NULL DEFAULT 0,
    reply_count INT NOT NULL DEFAULT 0,

    is_sensitive BIT NOT NULL DEFAULT 0,
    is_edited BIT NOT NULL DEFAULT 0,

    scheduled_for DATETIME2 NULL,
    published_at DATETIME2 NULL,

    edited_at DATETIME2 NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    deleted_at DATETIME2 NULL,

    CONSTRAINT fk_posts_profile
        FOREIGN KEY (profile_id)
        REFERENCES profiles(id),

    CONSTRAINT fk_posts_community
        FOREIGN KEY (community_id)
        REFERENCES communities(id),

    CONSTRAINT fk_posts_parent
        FOREIGN KEY (parent_post_id)
        REFERENCES posts(id),

    CONSTRAINT fk_posts_quoted
        FOREIGN KEY (quoted_post_id)
        REFERENCES posts(id),

    CONSTRAINT chk_posts_visibility
        CHECK (visibility IN ('public', 'followers', 'private'))
);

GO

ALTER TABLE profiles
ADD CONSTRAINT fk_profiles_pinned_post
FOREIGN KEY (pinned_post_id)
REFERENCES posts(id);

GO

CREATE INDEX ix_posts_profile_created
ON posts(profile_id, created_at DESC);

CREATE INDEX ix_posts_community_created
ON posts(community_id, created_at DESC);

CREATE INDEX ix_posts_parent_post
ON posts(parent_post_id);

CREATE INDEX ix_posts_created
ON posts(created_at DESC);

GO

/*
====================================================================
 POST REPOSTS
====================================================================
*/

CREATE TABLE post_reposts (
    post_id UNIQUEIDENTIFIER NOT NULL,
    profile_id UNIQUEIDENTIFIER NOT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_post_reposts
        PRIMARY KEY (post_id, profile_id),

    CONSTRAINT fk_post_reposts_post
        FOREIGN KEY (post_id)
        REFERENCES posts(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_post_reposts_profile
        FOREIGN KEY (profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE
);

GO

/*
====================================================================
 POST MEDIA
====================================================================
*/

CREATE TABLE post_media (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    post_id UNIQUEIDENTIFIER NOT NULL,

    media_type NVARCHAR(20) NOT NULL,

    url NVARCHAR(1000) NOT NULL,
    public_id NVARCHAR(500) NOT NULL,

    width INT NULL,
    height INT NULL,

    duration_seconds FLOAT NULL,

    mime_type NVARCHAR(100) NULL,

    size_bytes BIGINT NULL,

    position INT NOT NULL DEFAULT 0,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_post_media_post
        FOREIGN KEY (post_id)
        REFERENCES posts(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_post_media_type
        CHECK (media_type IN ('image', 'video'))
);

GO

/*
====================================================================
 HASHTAGS
====================================================================
*/

CREATE TABLE hashtags (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    tag NVARCHAR(100) NOT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

GO

CREATE UNIQUE INDEX ux_hashtags_tag
ON hashtags(tag);

GO

/*
====================================================================
 POST HASHTAGS
====================================================================
*/

CREATE TABLE post_hashtags (
    post_id UNIQUEIDENTIFIER NOT NULL,
    hashtag_id UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT pk_post_hashtags
        PRIMARY KEY (post_id, hashtag_id),

    CONSTRAINT fk_post_hashtags_post
        FOREIGN KEY (post_id)
        REFERENCES posts(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_post_hashtags_hashtag
        FOREIGN KEY (hashtag_id)
        REFERENCES hashtags(id)
        ON DELETE CASCADE
);

GO

/*
====================================================================
 POST MENTIONS
====================================================================
*/

CREATE TABLE post_mentions (
    post_id UNIQUEIDENTIFIER NOT NULL,
    mentioned_profile_id UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT pk_post_mentions
        PRIMARY KEY (post_id, mentioned_profile_id),

    CONSTRAINT fk_post_mentions_post
        FOREIGN KEY (post_id)
        REFERENCES posts(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_post_mentions_profile
        FOREIGN KEY (mentioned_profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE
);

GO

/*
====================================================================
 POST LIKES
====================================================================
*/

CREATE TABLE post_likes (
    post_id UNIQUEIDENTIFIER NOT NULL,
    profile_id UNIQUEIDENTIFIER NOT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_post_likes
        PRIMARY KEY (post_id, profile_id),

    CONSTRAINT fk_post_likes_post
        FOREIGN KEY (post_id)
        REFERENCES posts(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_post_likes_profile
        FOREIGN KEY (profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE
);

GO

/*
====================================================================
 POST BOOKMARKS
====================================================================
*/

CREATE TABLE post_bookmarks (
    post_id UNIQUEIDENTIFIER NOT NULL,
    profile_id UNIQUEIDENTIFIER NOT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_post_bookmarks
        PRIMARY KEY (post_id, profile_id),

    CONSTRAINT fk_post_bookmarks_post
        FOREIGN KEY (post_id)
        REFERENCES posts(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_post_bookmarks_profile
        FOREIGN KEY (profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE
);

GO

/*
====================================================================
 CONVERSATIONS
====================================================================
*/

CREATE TABLE conversations (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

GO

/*
====================================================================
 CONVERSATION PARTICIPANTS
====================================================================
*/

CREATE TABLE conversation_participants (
    conversation_id UNIQUEIDENTIFIER NOT NULL,
    profile_id UNIQUEIDENTIFIER NOT NULL,

    joined_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_conversation_participants
        PRIMARY KEY (conversation_id, profile_id),

    CONSTRAINT fk_conversation_participants_conversation
        FOREIGN KEY (conversation_id)
        REFERENCES conversations(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_conversation_participants_profile
        FOREIGN KEY (profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE
);

GO

/*
====================================================================
 MESSAGES
====================================================================
*/

CREATE TABLE messages (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    conversation_id UNIQUEIDENTIFIER NOT NULL,

    sender_profile_id UNIQUEIDENTIFIER NOT NULL,

    content NVARCHAR(2000) NULL,

    is_seen BIT NOT NULL DEFAULT 0,

    is_edited BIT NOT NULL DEFAULT 0,

    edited_at DATETIME2 NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    deleted_at DATETIME2 NULL,

    CONSTRAINT fk_messages_conversation
        FOREIGN KEY (conversation_id)
        REFERENCES conversations(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_messages_sender
        FOREIGN KEY (sender_profile_id)
        REFERENCES profiles(id)
);

GO

CREATE INDEX ix_messages_conversation_created
ON messages(conversation_id, created_at DESC);

GO

/*
====================================================================
 MESSAGE MEDIA
====================================================================
*/

CREATE TABLE message_media (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    message_id UNIQUEIDENTIFIER NOT NULL,

    media_type NVARCHAR(20) NOT NULL,

    url NVARCHAR(1000) NOT NULL,
    public_id NVARCHAR(500) NOT NULL,

    width INT NULL,
    height INT NULL,

    duration_seconds FLOAT NULL,

    mime_type NVARCHAR(100) NULL,

    size_bytes BIGINT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_message_media_message
        FOREIGN KEY (message_id)
        REFERENCES messages(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_message_media_type
        CHECK (media_type IN ('image', 'video'))
);

GO

/*
====================================================================
 NOTIFICATIONS
====================================================================
*/

CREATE TABLE notifications (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    recipient_profile_id UNIQUEIDENTIFIER NOT NULL,

    actor_profile_id UNIQUEIDENTIFIER NULL,

    type NVARCHAR(50) NOT NULL,

    reference_id UNIQUEIDENTIFIER NULL,

    is_read BIT NOT NULL DEFAULT 0,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_notifications_recipient
        FOREIGN KEY (recipient_profile_id)
        REFERENCES profiles(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_notifications_actor
        FOREIGN KEY (actor_profile_id)
        REFERENCES profiles(id)
);

GO

CREATE INDEX ix_notifications_recipient_created
ON notifications(recipient_profile_id, created_at DESC);

GO

/*
====================================================================
 REPORTS
====================================================================
*/

CREATE TABLE reports (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    reporter_profile_id UNIQUEIDENTIFIER NOT NULL,

    target_type NVARCHAR(20) NOT NULL,
    target_id UNIQUEIDENTIFIER NOT NULL,

    reason NVARCHAR(500) NOT NULL,

    status NVARCHAR(20) NOT NULL DEFAULT 'pending',

    reviewed_by_profile_id UNIQUEIDENTIFIER NULL,

    resolution NVARCHAR(1000) NULL,

    reviewed_at DATETIME2 NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_reports_reporter
        FOREIGN KEY (reporter_profile_id)
        REFERENCES profiles(id),

    CONSTRAINT fk_reports_reviewer
        FOREIGN KEY (reviewed_by_profile_id)
        REFERENCES profiles(id),

    CONSTRAINT chk_reports_target_type
        CHECK (target_type IN ('profile', 'post', 'message', 'community')),

    CONSTRAINT chk_reports_status
        CHECK (status IN ('pending', 'reviewing', 'resolved', 'rejected'))
);

GO

/*
====================================================================
 USER SESSIONS
====================================================================
*/

CREATE TABLE user_sessions (
    id UNIQUEIDENTIFIER PRIMARY KEY,

    auth_user_id UNIQUEIDENTIFIER NOT NULL,

    refresh_token_hash NVARCHAR(500) NOT NULL,

    expires_at DATETIME2 NOT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT fk_user_sessions_auth_user
        FOREIGN KEY (auth_user_id)
        REFERENCES auth_users(id)
        ON DELETE CASCADE
);

GO

CREATE INDEX ix_user_sessions_auth_user
ON user_sessions(auth_user_id);

GO
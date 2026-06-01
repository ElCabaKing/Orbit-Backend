using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "conversation_participants",
                columns: table => new
                {
                    conversation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    joined_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_participants", x => new { x.conversation_id, x.profile_id });
                    table.ForeignKey(
                        name: "FK_conversation_participants_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_conversation_participants_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    sender_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    is_seen = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_edited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    edited_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_messages_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_profiles_sender_profile_id",
                        column: x => x.sender_profile_id,
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "message_media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    message_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    media_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    public_id = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    width = table.Column<int>(type: "int", nullable: true),
                    height = table.Column<int>(type: "int", nullable: true),
                    duration_seconds = table.Column<double>(type: "float", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_message_media_messages_message_id",
                        column: x => x.message_id,
                        principalTable: "messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_conversation_participants_profile_id",
                table: "conversation_participants",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_media_message_id",
                table: "message_media",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_conversation_created",
                table: "messages",
                columns: new[] { "conversation_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_messages_conversation_id",
                table: "messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_sender_profile_id",
                table: "messages",
                column: "sender_profile_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conversation_participants");

            migrationBuilder.DropTable(
                name: "message_media");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "conversations");
        }
    }
}

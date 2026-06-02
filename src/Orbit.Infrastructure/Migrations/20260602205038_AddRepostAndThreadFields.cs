using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRepostAndThreadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_posts_posts_original_post_id')
                    ALTER TABLE [posts] DROP CONSTRAINT [FK_posts_posts_original_post_id];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_posts_original_post_id' AND object_id = OBJECT_ID('posts'))
                    DROP INDEX [ix_posts_original_post_id] ON [posts];
                IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'is_repost' AND object_id = OBJECT_ID('posts'))
                    ALTER TABLE [posts] DROP COLUMN [is_repost];
                IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'is_thread' AND object_id = OBJECT_ID('posts'))
                    ALTER TABLE [posts] DROP COLUMN [is_thread];
                IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'original_post_id' AND object_id = OBJECT_ID('posts'))
                    ALTER TABLE [posts] DROP COLUMN [original_post_id];
            """);

            migrationBuilder.AddColumn<bool>(
                name: "is_repost",
                table: "posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_thread",
                table: "posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "original_post_id",
                table: "posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_original_post_id",
                table: "posts",
                column: "original_post_id");

            migrationBuilder.AddForeignKey(
                name: "FK_posts_posts_original_post_id",
                table: "posts",
                column: "original_post_id",
                principalTable: "posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_posts_posts_original_post_id",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_posts_original_post_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "is_repost",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "is_thread",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "original_post_id",
                table: "posts");
        }
    }
}

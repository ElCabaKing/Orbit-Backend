using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityIdToPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "community_id",
                table: "posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_community_id",
                table: "posts",
                column: "community_id");

            migrationBuilder.AddForeignKey(
                name: "FK_posts_communities_community_id",
                table: "posts",
                column: "community_id",
                principalTable: "communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_posts_communities_community_id",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_posts_community_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "community_id",
                table: "posts");
        }
    }
}

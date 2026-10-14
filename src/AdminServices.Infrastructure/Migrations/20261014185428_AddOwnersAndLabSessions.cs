using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminServices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnersAndLabSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SsoSub",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "StorageFolders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Notes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "LocalDocuments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SsoSid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_SsoSub",
                table: "Users",
                column: "SsoSub",
                unique: true,
                filter: "\"SsoSub\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StorageFolders_OwnerId",
                table: "StorageFolders",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_OwnerId",
                table: "Notes",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDocuments_OwnerId",
                table: "LocalDocuments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_OwnerId",
                table: "Categories",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_Users_SsoSub",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_StorageFolders_OwnerId",
                table: "StorageFolders");

            migrationBuilder.DropIndex(
                name: "IX_Notes_OwnerId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_LocalDocuments_OwnerId",
                table: "LocalDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Categories_OwnerId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SsoSub",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "StorageFolders");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "LocalDocuments");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Categories");
        }
    }
}

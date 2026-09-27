using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace soromaps_api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "biography",
                table: "users",
                type: "character varying(280)",
                maxLength: 280,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "neighborhood",
                table: "users",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "biography",
                table: "users");

            migrationBuilder.DropColumn(
                name: "neighborhood",
                table: "users");
        }
    }
}

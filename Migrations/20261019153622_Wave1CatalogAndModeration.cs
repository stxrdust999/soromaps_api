using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace soromaps_api.Migrations
{
    /// <inheritdoc />
    public partial class Wave1CatalogAndModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "best_time",
                table: "tb_places",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "local_secret",
                table: "tb_places",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "tb_categories",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#2563EB");

            migrationBuilder.AddColumn<string>(
                name: "icon",
                table: "tb_categories",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "map-pin");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "tb_categories",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "tb_categories",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "sort_order",
                table: "tb_categories",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "tb_moderation_decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    moderator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    previous_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    feedback_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_undo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_moderation_decisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_tb_moderation_decisions_places_place_id",
                        column: x => x.place_id,
                        principalTable: "tb_places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_moderation_decisions_users_moderator_id",
                        column: x => x.moderator_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_place_photos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    url = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    storage_path = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    is_cover = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_place_photos", x => x.id);
                    table.ForeignKey(
                        name: "fk_tb_place_photos_tb_places_place_id",
                        column: x => x.place_id,
                        principalTable: "tb_places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_place_photos_users_uploaded_by_id",
                        column: x => x.uploaded_by_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_place_tags",
                columns: table => new
                {
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_place_tags", x => new { x.place_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_tb_place_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tb_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_place_tags_tb_places_place_id",
                        column: x => x.place_id,
                        principalTable: "tb_places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tb_places_latitude_longitude",
                table: "tb_places",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "ix_tb_places_neighborhood",
                table: "tb_places",
                column: "neighborhood");

            migrationBuilder.CreateIndex(
                name: "ix_tb_categories_is_active_sort_order",
                table: "tb_categories",
                columns: new[] { "is_active", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_tb_categories_slug",
                table: "tb_categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_moderation_decisions_created_at",
                table: "tb_moderation_decisions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tb_moderation_decisions_moderator_id",
                table: "tb_moderation_decisions",
                column: "moderator_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_moderation_decisions_place_id_created_at",
                table: "tb_moderation_decisions",
                columns: new[] { "place_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tb_place_photos_place_id",
                table: "tb_place_photos",
                column: "place_id",
                unique: true,
                filter: "is_cover = true");

            migrationBuilder.CreateIndex(
                name: "ix_tb_place_photos_uploaded_by_id",
                table: "tb_place_photos",
                column: "uploaded_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_place_tags_tag_id",
                table: "tb_place_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_tags_name",
                table: "tb_tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_tags_slug",
                table: "tb_tags",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_moderation_decisions");

            migrationBuilder.DropTable(
                name: "tb_place_photos");

            migrationBuilder.DropTable(
                name: "tb_place_tags");

            migrationBuilder.DropTable(
                name: "tb_tags");

            migrationBuilder.DropIndex(
                name: "ix_tb_places_latitude_longitude",
                table: "tb_places");

            migrationBuilder.DropIndex(
                name: "ix_tb_places_neighborhood",
                table: "tb_places");

            migrationBuilder.DropIndex(
                name: "ix_tb_categories_is_active_sort_order",
                table: "tb_categories");

            migrationBuilder.DropIndex(
                name: "ix_tb_categories_slug",
                table: "tb_categories");

            migrationBuilder.DropColumn(
                name: "best_time",
                table: "tb_places");

            migrationBuilder.DropColumn(
                name: "local_secret",
                table: "tb_places");

            migrationBuilder.DropColumn(
                name: "color",
                table: "tb_categories");

            migrationBuilder.DropColumn(
                name: "icon",
                table: "tb_categories");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "tb_categories");

            migrationBuilder.DropColumn(
                name: "slug",
                table: "tb_categories");

            migrationBuilder.DropColumn(
                name: "sort_order",
                table: "tb_categories");
        }
    }
}

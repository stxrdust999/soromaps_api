using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace soromaps_api.Migrations
{
    /// <inheritdoc />
    public partial class Wave2ReviewsActivityGamification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tb_reviews_place_id_user_id",
                table: "tb_reviews");

            migrationBuilder.AlterColumn<int>(
                name: "rating",
                table: "tb_reviews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<DateTime>(
                name: "removed_at",
                table: "tb_reviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "removed_at",
                table: "tb_comments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "reply_to_id",
                table: "tb_comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "tb_achievements",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "#F59E0B");

            migrationBuilder.AddColumn<string>(
                name: "event_type",
                table: "tb_achievements",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Visit");

            migrationBuilder.AddColumn<string>(
                name: "icon",
                table: "tb_achievements",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "award");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "tb_achievements",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "required_quantity",
                table: "tb_achievements",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "target_type",
                table: "tb_achievements",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Any");

            migrationBuilder.AddColumn<string>(
                name: "target_value",
                table: "tb_achievements",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_content_removals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    target_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: true),
                    moderator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    snapshot_text = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_content_removals", x => x.id);
                    table.ForeignKey(
                        name: "fk_tb_content_removals_users_author_id",
                        column: x => x.author_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_tb_content_removals_users_moderator_id",
                        column: x => x.moderator_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_favorites",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_favorites", x => new { x.user_id, x.place_id });
                    table.ForeignKey(
                        name: "fk_tb_favorites_places_place_id",
                        column: x => x.place_id,
                        principalTable: "tb_places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_favorites_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_review_useful_votes",
                columns: table => new
                {
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_review_useful_votes", x => new { x.review_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_tb_review_useful_votes_tb_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "tb_reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_review_useful_votes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_user_achievements",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    achievement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unlocked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_user_achievements", x => new { x.user_id, x.achievement_id });
                    table.ForeignKey(
                        name: "fk_tb_user_achievements_tb_achievements_achievement_id",
                        column: x => x.achievement_id,
                        principalTable: "tb_achievements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_user_achievements_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_visits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    visited_on = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_visits", x => x.id);
                    table.ForeignKey(
                        name: "fk_tb_visits_tb_places_place_id",
                        column: x => x.place_id,
                        principalTable: "tb_places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_visits_tb_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tb_reviews_place_id_created_at",
                table: "tb_reviews",
                columns: new[] { "place_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tb_reviews_place_id_user_id",
                table: "tb_reviews",
                columns: new[] { "place_id", "user_id" },
                unique: true,
                filter: "removed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tb_comments_reply_to_id",
                table: "tb_comments",
                column: "reply_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_achievements_is_active_event_type",
                table: "tb_achievements",
                columns: new[] { "is_active", "event_type" });

            migrationBuilder.CreateIndex(
                name: "ix_tb_content_removals_author_id",
                table: "tb_content_removals",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_content_removals_created_at",
                table: "tb_content_removals",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tb_content_removals_moderator_id",
                table: "tb_content_removals",
                column: "moderator_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_content_removals_target_type_target_id",
                table: "tb_content_removals",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tb_favorites_place_id",
                table: "tb_favorites",
                column: "place_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_review_useful_votes_user_id",
                table: "tb_review_useful_votes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_user_achievements_achievement_id",
                table: "tb_user_achievements",
                column: "achievement_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_visits_place_id",
                table: "tb_visits",
                column: "place_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_visits_user_id_place_id_visited_on",
                table: "tb_visits",
                columns: new[] { "user_id", "place_id", "visited_on" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_visits_user_id_visited_on",
                table: "tb_visits",
                columns: new[] { "user_id", "visited_on" });

            migrationBuilder.AddForeignKey(
                name: "fk_tb_comments_tb_comments_reply_to_id",
                table: "tb_comments",
                column: "reply_to_id",
                principalTable: "tb_comments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tb_comments_tb_comments_reply_to_id",
                table: "tb_comments");

            migrationBuilder.DropTable(
                name: "tb_content_removals");

            migrationBuilder.DropTable(
                name: "tb_favorites");

            migrationBuilder.DropTable(
                name: "tb_review_useful_votes");

            migrationBuilder.DropTable(
                name: "tb_user_achievements");

            migrationBuilder.DropTable(
                name: "tb_visits");

            migrationBuilder.DropIndex(
                name: "ix_tb_reviews_place_id_created_at",
                table: "tb_reviews");

            migrationBuilder.DropIndex(
                name: "ix_tb_reviews_place_id_user_id",
                table: "tb_reviews");

            migrationBuilder.DropIndex(
                name: "ix_tb_comments_reply_to_id",
                table: "tb_comments");

            migrationBuilder.DropIndex(
                name: "ix_tb_achievements_is_active_event_type",
                table: "tb_achievements");

            migrationBuilder.DropColumn(
                name: "removed_at",
                table: "tb_reviews");

            migrationBuilder.DropColumn(
                name: "removed_at",
                table: "tb_comments");

            migrationBuilder.DropColumn(
                name: "reply_to_id",
                table: "tb_comments");

            migrationBuilder.DropColumn(
                name: "color",
                table: "tb_achievements");

            migrationBuilder.DropColumn(
                name: "event_type",
                table: "tb_achievements");

            migrationBuilder.DropColumn(
                name: "icon",
                table: "tb_achievements");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "tb_achievements");

            migrationBuilder.DropColumn(
                name: "required_quantity",
                table: "tb_achievements");

            migrationBuilder.DropColumn(
                name: "target_type",
                table: "tb_achievements");

            migrationBuilder.DropColumn(
                name: "target_value",
                table: "tb_achievements");

            migrationBuilder.AlterColumn<short>(
                name: "rating",
                table: "tb_reviews",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "ix_tb_reviews_place_id_user_id",
                table: "tb_reviews",
                columns: new[] { "place_id", "user_id" },
                unique: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace soromaps_api.Migrations
{
    /// <inheritdoc />
    public partial class Wave3ReportsFeedStoriesDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_feed_mutes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    scope_value = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_feed_mutes", x => x.id);
                    table.ForeignKey(
                        name: "fk_tb_feed_mutes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_feedback",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    origin_path = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_feedback", x => x.id);
                    table.ForeignKey(
                        name: "fk_tb_feedback_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    reporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    resolved_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolution_note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_tb_reports_users_reporter_id",
                        column: x => x.reporter_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_reports_users_resolved_by_id",
                        column: x => x.resolved_by_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_stories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    title = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    subtitle = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: false),
                    neighborhood = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    intro_text = table.Column<string>(type: "text", nullable: false),
                    closing_text = table.Column<string>(type: "text", nullable: true),
                    origin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_stories", x => x.id);
                    table.ForeignKey(
                        name: "fk_tb_stories_users_author_id",
                        column: x => x.author_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_story_places",
                columns: table => new
                {
                    story_id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<short>(type: "smallint", nullable: false),
                    angle_title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    body_text = table.Column<string>(type: "text", nullable: false),
                    local_tip = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_story_places", x => new { x.story_id, x.place_id });
                    table.ForeignKey(
                        name: "fk_tb_story_places_tb_places_place_id",
                        column: x => x.place_id,
                        principalTable: "tb_places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_story_places_tb_stories_story_id",
                        column: x => x.story_id,
                        principalTable: "tb_stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tb_feed_mutes_user_id",
                table: "tb_feed_mutes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_feed_mutes_user_id_scope_type_scope_value",
                table: "tb_feed_mutes",
                columns: new[] { "user_id", "scope_type", "scope_value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_feedback_created_at",
                table: "tb_feedback",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tb_feedback_user_id",
                table: "tb_feedback",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_reports_created_at",
                table: "tb_reports",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tb_reports_reporter_id_target_type_target_id",
                table: "tb_reports",
                columns: new[] { "reporter_id", "target_type", "target_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_reports_resolved_by_id",
                table: "tb_reports",
                column: "resolved_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_reports_status",
                table: "tb_reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_tb_reports_target_type_target_id",
                table: "tb_reports",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tb_stories_author_id",
                table: "tb_stories",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_stories_published_at",
                table: "tb_stories",
                column: "published_at");

            migrationBuilder.CreateIndex(
                name: "ix_tb_stories_slug",
                table: "tb_stories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_stories_status",
                table: "tb_stories",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_tb_story_places_place_id",
                table: "tb_story_places",
                column: "place_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_feed_mutes");

            migrationBuilder.DropTable(
                name: "tb_feedback");

            migrationBuilder.DropTable(
                name: "tb_reports");

            migrationBuilder.DropTable(
                name: "tb_story_places");

            migrationBuilder.DropTable(
                name: "tb_stories");
        }
    }
}

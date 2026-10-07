using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace soromaps_api.Migrations
{
    /// <inheritdoc />
    public partial class MigrationMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_achievements",
                columns: table => new
                {
                    achievement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    achievement_name = table.Column<string>(type: "text", nullable: false),
                    achievement_desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_achievements", x => x.achievement_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_categorys",
                columns: table => new
                {
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_categorys", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "explorer"),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    biography = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: true),
                    neighborhood = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_places",
                columns: table => new
                {
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_name = table.Column<string>(type: "text", nullable: false),
                    lat = table.Column<float>(type: "real", nullable: false),
                    lng = table.Column<float>(type: "real", nullable: false),
                    neighborhood = table.Column<string>(type: "text", nullable: false),
                    about = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    has_wifi = table.Column<bool>(type: "boolean", nullable: false),
                    pet_friendly = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_places", x => x.place_id);
                    table.ForeignKey(
                        name: "fk_tb_places_tb_categorys_category_id",
                        column: x => x.category_id,
                        principalTable: "tb_categorys",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_places_users_author_id",
                        column: x => x.author_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_sessions",
                columns: table => new
                {
                    session_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_agent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "fk_tb_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_reviews",
                columns: table => new
                {
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_content = table.Column<string>(type: "text", nullable: false),
                    qtd_stars = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_reviews", x => x.review_id);
                    table.ForeignKey(
                        name: "fk_tb_reviews_tb_places_place_id",
                        column: x => x.place_id,
                        principalTable: "tb_places",
                        principalColumn: "place_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_reviews_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_comments",
                columns: table => new
                {
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_comments", x => x.comment_id);
                    table.ForeignKey(
                        name: "fk_tb_comments_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "tb_reviews",
                        principalColumn: "review_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_comments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tb_comments_review_id",
                table: "tb_comments",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_comments_user_id",
                table: "tb_comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_places_author_id",
                table: "tb_places",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_places_category_id",
                table: "tb_places",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_reviews_place_id",
                table: "tb_reviews",
                column: "place_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_reviews_user_id",
                table: "tb_reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_sessions_token_hash",
                table: "tb_sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_sessions_user_id",
                table: "tb_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_users_email",
                table: "tb_users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_achievements");

            migrationBuilder.DropTable(
                name: "tb_comments");

            migrationBuilder.DropTable(
                name: "tb_sessions");

            migrationBuilder.DropTable(
                name: "tb_reviews");

            migrationBuilder.DropTable(
                name: "tb_places");

            migrationBuilder.DropTable(
                name: "tb_categorys");

            migrationBuilder.DropTable(
                name: "tb_users");
        }
    }
}

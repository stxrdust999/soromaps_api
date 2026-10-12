using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace soromaps_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialRefactoredSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_achievements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_achievements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Explorer"),
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
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    neighborhood = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    about = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    has_wifi = table.Column<bool>(type: "boolean", nullable: false),
                    is_pet_friendly = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_places", x => x.id);
                    table.ForeignKey(
                        name: "fk_tb_places_tb_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "tb_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tb_places_users_author_id",
                        column: x => x.author_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_sessions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
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
                    table.PrimaryKey("pk_tb_sessions", x => x.id);
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
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    rating = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_reviews", x => x.id);
                    table.CheckConstraint("ck_tb_reviews_rating", "rating >= 1 AND rating <= 5");
                    table.ForeignKey(
                        name: "fk_tb_reviews_tb_places_place_id",
                        column: x => x.place_id,
                        principalTable: "tb_places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_reviews_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_tb_comments_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "tb_reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tb_comments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tb_achievements_name",
                table: "tb_achievements",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_categories_name",
                table: "tb_categories",
                column: "name",
                unique: true);

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
                name: "ix_tb_places_status_created_at",
                table: "tb_places",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tb_reviews_place_id_user_id",
                table: "tb_reviews",
                columns: new[] { "place_id", "user_id" },
                unique: true);

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
                name: "tb_categories");

            migrationBuilder.DropTable(
                name: "tb_users");
        }
    }
}

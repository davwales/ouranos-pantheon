using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ouranos.Pantheon.Hermes.Service.Infra.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hermes");

            migrationBuilder.CreateTable(
                name: "assistants",
                schema: "hermes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    model = table.Column<string>(type: "text", nullable: false),
                    system_prompt = table.Column<string>(type: "text", nullable: false),
                    assistant_name = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    temperature = table.Column<float>(type: "real", nullable: true),
                    max_tokens = table.Column<int>(type: "integer", nullable: true),
                    repeat_penalty = table.Column<float>(type: "real", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assistants", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assistants",
                schema: "hermes");
        }
    }
}

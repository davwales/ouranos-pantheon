using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.Migrations;

/// <inheritdoc />
public partial class InitialSetup : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "plutus");

        migrationBuilder.CreateTable(
            name: "markets",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                taxes_flat_minimum = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: true
                ),
                taxes_flat_maximum = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: true
                ),
                taxes_flat_rate = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: true
                ),
                is_forecasting_enabled = table.Column<bool>(type: "boolean", nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                icon = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_markets", x => x.id);
            }
        );

        migrationBuilder.CreateTable(
            name: "trade_message",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                trade_id = table.Column<Guid>(type: "uuid", nullable: false),
                message_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_trade_message", x => x.id);
            }
        );

        migrationBuilder.CreateTable(
            name: "recipes",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                cost = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipes", x => x.id);
                table.ForeignKey(
                    name: "fk_recipes_markets_market_id",
                    column: x => x.market_id,
                    principalSchema: "plutus",
                    principalTable: "markets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "symbols",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "text", nullable: false),
                subcode = table.Column<string>(type: "text", nullable: true),
                name = table.Column<string>(type: "text", nullable: false),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
                additional_fields_limit = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: true
                ),
                additional_fields_high_alch = table.Column<int>(type: "integer", nullable: true),
                additional_fields_low_alch = table.Column<int>(type: "integer", nullable: true),
                additional_fields_exchange = table.Column<string>(type: "text", nullable: true),
                additional_fields_tape = table.Column<string>(type: "text", nullable: true),
                additional_fields_external_trade_id = table.Column<string>(
                    type: "text",
                    nullable: true
                ),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_symbols", x => x.id);
                table.ForeignKey(
                    name: "fk_symbols_markets_market_id",
                    column: x => x.market_id,
                    principalSchema: "plutus",
                    principalTable: "markets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "recipes_inputs",
            schema: "plutus",
            columns: table => new
            {
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                symbol_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipes_inputs", x => new { x.recipe_id, x.id });
                table.ForeignKey(
                    name: "fk_recipes_inputs_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "plutus",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "recipes_outputs",
            schema: "plutus",
            columns: table => new
            {
                recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                symbol_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recipes_outputs", x => new { x.recipe_id, x.id });
                table.ForeignKey(
                    name: "fk_recipes_outputs_recipes_recipe_id",
                    column: x => x.recipe_id,
                    principalSchema: "plutus",
                    principalTable: "recipes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "forecasts",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                market_id = table.Column<Guid>(type: "uuid", nullable: false),
                symbol_id = table.Column<Guid>(type: "uuid", nullable: false),
                latest_average_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                latest_min_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                latest_max_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                latest_volume = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_forecasts", x => x.id);
                table.ForeignKey(
                    name: "fk_forecasts_symbols_symbol_id",
                    column: x => x.symbol_id,
                    principalSchema: "plutus",
                    principalTable: "symbols",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "trades",
            schema: "plutus",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                timestamp = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                symbol_id = table.Column<Guid>(type: "uuid", nullable: false),
                price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                volume = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                created_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                updated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_trades", x => new { x.id, x.timestamp });
                table.ForeignKey(
                    name: "fk_trades_symbols_symbol_id",
                    column: x => x.symbol_id,
                    principalSchema: "plutus",
                    principalTable: "symbols",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "forecasts_predictions",
            schema: "plutus",
            columns: table => new
            {
                forecast_id = table.Column<Guid>(type: "uuid", nullable: false),
                id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                average_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                min_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                max_price = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
                volume = table.Column<decimal>(
                    type: "numeric(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_forecasts_predictions", x => new { x.forecast_id, x.id });
                table.ForeignKey(
                    name: "fk_forecasts_predictions_forecasts_forecast_id",
                    column: x => x.forecast_id,
                    principalSchema: "plutus",
                    principalTable: "forecasts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_forecasts_symbol_id",
            schema: "plutus",
            table: "forecasts",
            column: "symbol_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_recipes_market_id",
            schema: "plutus",
            table: "recipes",
            column: "market_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_symbols_code_subcode_market_id",
            schema: "plutus",
            table: "symbols",
            columns: new[] { "code", "subcode", "market_id" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "ix_symbols_market_id",
            schema: "plutus",
            table: "symbols",
            column: "market_id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_trade_message_message_id",
            schema: "plutus",
            table: "trade_message",
            column: "message_id",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "ix_trades_symbol_id",
            schema: "plutus",
            table: "trades",
            column: "symbol_id"
        );

        migrationBuilder.Sql(
            "SELECT create_hypertable('\"plutus\".\"trades\"', 'timestamp', migrate_data => TRUE);\n"
                + "CREATE INDEX ix_symbol_id_timestamp ON \"plutus\".\"trades\" (\"symbol_id\", \"timestamp\" DESC);"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "forecasts_predictions", schema: "plutus");

        migrationBuilder.DropTable(name: "recipes_inputs", schema: "plutus");

        migrationBuilder.DropTable(name: "recipes_outputs", schema: "plutus");

        migrationBuilder.DropTable(name: "trade_message", schema: "plutus");

        migrationBuilder.DropTable(name: "trades", schema: "plutus");

        migrationBuilder.DropTable(name: "forecasts", schema: "plutus");

        migrationBuilder.DropTable(name: "recipes", schema: "plutus");

        migrationBuilder.DropTable(name: "symbols", schema: "plutus");

        migrationBuilder.DropTable(name: "markets", schema: "plutus");
    }
}

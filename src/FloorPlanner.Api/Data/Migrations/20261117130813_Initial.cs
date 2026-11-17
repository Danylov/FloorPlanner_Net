using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FloorPlanner.Api.Data.Migrations;

/// <inheritdoc />
public partial class _20261117130813_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "floor_plans",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_floor_plans", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "rooms",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                floor_plan_id = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                height = table.Column<double>(type: "double precision", nullable: false),
                corners = table.Column<string>(type: "jsonb", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_rooms", x => x.id);
                table.ForeignKey(
                    name: "fk_rooms_floor_plans_floor_plan_id",
                    column: x => x.floor_plan_id,
                    principalTable: "floor_plans",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "plan_objects",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                x = table.Column<double>(type: "double precision", nullable: false),
                z = table.Column<double>(type: "double precision", nullable: false),
                width = table.Column<double>(type: "double precision", nullable: false),
                depth = table.Column<double>(type: "double precision", nullable: false),
                height = table.Column<double>(type: "double precision", nullable: false),
                room_id = table.Column<int>(type: "integer", nullable: false),
                kind = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_plan_objects", x => x.id);
                table.ForeignKey(
                    name: "fk_plan_objects_rooms_room_id",
                    column: x => x.room_id,
                    principalTable: "rooms",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_plan_objects_room_id",
            table: "plan_objects",
            column: "room_id");

        migrationBuilder.CreateIndex(
            name: "ix_rooms_floor_plan_id",
            table: "rooms",
            column: "floor_plan_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "plan_objects");

        migrationBuilder.DropTable(
            name: "rooms");

        migrationBuilder.DropTable(
            name: "floor_plans");
    }
}

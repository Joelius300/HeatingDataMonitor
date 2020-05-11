using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HeatingDataMonitor.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeatingData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Zeit = table.Column<DateTime>(nullable: false),
                    Kessel = table.Column<float>(nullable: true),
                    Ruecklauf = table.Column<float>(nullable: true),
                    Abgas = table.Column<float>(nullable: true),
                    Brennkammer = table.Column<float>(nullable: true),
                    CO2_Soll = table.Column<float>(nullable: true),
                    CO2_Ist = table.Column<float>(nullable: true),
                    Saugzug_Ist = table.Column<float>(nullable: true),
                    Puffer_Oben = table.Column<float>(nullable: true),
                    Puffer_Unten = table.Column<float>(nullable: true),
                    Platine = table.Column<float>(nullable: true),
                    Betriebsphase_Kessel = table.Column<int>(nullable: true),
                    Aussen = table.Column<float>(nullable: true),
                    Vorlauf_HK1_Ist = table.Column<float>(nullable: true),
                    Vorlauf_HK1_Soll = table.Column<float>(nullable: true),
                    Betriebsphase_HK1 = table.Column<int>(nullable: true),
                    Betriebsart_Fern_HK1 = table.Column<int>(nullable: true),
                    Verschiebung_Fern_HK1 = table.Column<float>(nullable: true),
                    Freigabekontakt_HK1 = table.Column<float>(nullable: true),
                    Vorlauf_HK2_Ist = table.Column<float>(nullable: true),
                    Vorlauf_HK2_Soll = table.Column<float>(nullable: true),
                    Betriebsphase_HK2 = table.Column<int>(nullable: true),
                    Betriebsart_Fern_HK2 = table.Column<int>(nullable: true),
                    Verschiebung_Fern_HK2 = table.Column<float>(nullable: true),
                    Freigabekontakt_HK2 = table.Column<float>(nullable: true),
                    Vorlauf_HK3_Ist = table.Column<float>(nullable: true),
                    Vorlauf_HK3_Soll = table.Column<float>(nullable: true),
                    Betriebsphase_HK3 = table.Column<int>(nullable: true),
                    Betriebsart_Fern_HK3 = table.Column<int>(nullable: true),
                    Verschiebung_Fern_HK3 = table.Column<float>(nullable: true),
                    Freigabekontakt_HK3 = table.Column<float>(nullable: true),
                    Vorlauf_HK4_Ist = table.Column<float>(nullable: true),
                    Vorlauf_HK4_Soll = table.Column<float>(nullable: true),
                    Betriebsphase_HK4 = table.Column<int>(nullable: true),
                    Betriebsart_Fern_HK4 = table.Column<int>(nullable: true),
                    Verschiebung_Fern_HK4 = table.Column<float>(nullable: true),
                    Freigabekontakt_HK4 = table.Column<float>(nullable: true),
                    Boiler_1 = table.Column<float>(nullable: true),
                    Boiler_2 = table.Column<float>(nullable: true),
                    DI_0 = table.Column<int>(nullable: true),
                    DI_1 = table.Column<int>(nullable: true),
                    DI_2 = table.Column<int>(nullable: true),
                    DI_3 = table.Column<int>(nullable: true),
                    A_W_0 = table.Column<int>(nullable: true),
                    A_W_1 = table.Column<int>(nullable: true),
                    A_W_2 = table.Column<int>(nullable: true),
                    A_W_3 = table.Column<int>(nullable: true),
                    A_EA_0 = table.Column<int>(nullable: true),
                    A_EA_1 = table.Column<int>(nullable: true),
                    A_EA_2 = table.Column<int>(nullable: true),
                    A_EA_3 = table.Column<int>(nullable: true),
                    A_EA_4 = table.Column<int>(nullable: true),
                    A_PHASE_0 = table.Column<int>(nullable: true),
                    A_PHASE_1 = table.Column<int>(nullable: true),
                    A_PHASE_2 = table.Column<int>(nullable: true),
                    A_PHASE_3 = table.Column<int>(nullable: true),
                    A_PHASE_4 = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeatingData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeatingData_Zeit",
                table: "HeatingData",
                column: "Zeit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeatingData");
        }
    }
}

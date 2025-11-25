using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace G.Sync.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Colunas_alteradas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VaultName",
                table: "Tasks");

            migrationBuilder.AddColumn<long>(
                name: "VaultId",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VaultId",
                table: "Tasks");

            migrationBuilder.AddColumn<string>(
                name: "VaultName",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}

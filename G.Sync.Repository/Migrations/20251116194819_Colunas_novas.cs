using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace G.Sync.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Colunas_novas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VaultName",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VaultName",
                table: "Tasks");
        }
    }
}

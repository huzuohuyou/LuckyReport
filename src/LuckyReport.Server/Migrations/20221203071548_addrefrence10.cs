using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuckyReport.Server.Migrations
{
    /// <inheritdoc />
    public partial class addrefrence10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataSourceId",
                table: "Reports",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataSourceId",
                table: "Reports");
        }
    }
}

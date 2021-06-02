using Microsoft.EntityFrameworkCore.Migrations;

namespace FalconMVC.Migrations
{
    public partial class add_TypeToGA : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "GAs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "GAs");
        }
    }
}

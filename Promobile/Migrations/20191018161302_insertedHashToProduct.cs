using Microsoft.EntityFrameworkCore.Migrations;

namespace Promobile.Migrations
{
    public partial class insertedHashToProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductHash",
                table: "PromotionalProducts",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductHash",
                table: "PromotionalProducts");
        }
    }
}

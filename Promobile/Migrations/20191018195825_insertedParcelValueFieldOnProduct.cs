using Microsoft.EntityFrameworkCore.Migrations;

namespace Promobile.Migrations
{
    public partial class insertedParcelValueFieldOnProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ParcelValue",
                table: "PromotionalProducts",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParcelValue",
                table: "PromotionalProducts");
        }
    }
}

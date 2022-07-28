using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IT_Next.Infrastructure.Data.Migrations;

public partial class CreateProductsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Price = table.Column<float>(type: "real", nullable: false),
                Discount = table.Column<float>(type: "real", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                SubCategoryId = table.Column<int>(type: "int", nullable: false),
                BrandId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
                table.ForeignKey(
                    name: "FK_Products_Brands_BrandId",
                    column: x => x.BrandId,
                    principalTable: "Brands",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Products_SubCategories_SubCategoryId",
                    column: x => x.SubCategoryId,
                    principalTable: "SubCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_BrandId",
            table: "Products",
            column: "BrandId");

        migrationBuilder.CreateIndex(
            name: "IX_Products_Name",
            table: "Products",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Products_SubCategoryId",
            table: "Products",
            column: "SubCategoryId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Products");
    }
}
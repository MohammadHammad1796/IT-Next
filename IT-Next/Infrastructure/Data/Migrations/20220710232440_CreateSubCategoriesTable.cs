using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IT_Next.Infrastructure.Data.Migrations;

public partial class CreateSubCategoriesTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SubCategories",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CategoryId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubCategories", x => x.Id);
                table.ForeignKey(
                    name: "FK_SubCategories_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SubCategories_CategoryId",
            table: "SubCategories",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_SubCategories_Name",
            table: "SubCategories",
            column: "Name",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SubCategories");
    }
}
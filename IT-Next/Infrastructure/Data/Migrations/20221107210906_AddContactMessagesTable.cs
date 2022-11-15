using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IT_Next.Infrastructure.Data.Migrations;

public partial class AddContactMessagesTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ContactMessages",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                MobileNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                Message = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsRead = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContactMessages", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ContactMessages");
    }
}
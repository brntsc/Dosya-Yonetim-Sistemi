using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uyg.API.Migrations
{
    /// <inheritdoc />
    public partial class mig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cerated",
                table: "Products",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "Cerated",
                table: "Categories",
                newName: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Products",
                newName: "Cerated");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Categories",
                newName: "Cerated");
        }
    }
}

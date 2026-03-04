using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionEcommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Vouchers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_ProductId",
                table: "Vouchers",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Products",
                table: "Vouchers",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Products",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_ProductId",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Vouchers");
        }
    }
}

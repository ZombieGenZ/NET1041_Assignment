using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment.Migrations
{
    /// <inheritdoc />
    public partial class v22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Products_ProductsId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ProductsId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductsId",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProductsId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductsId",
                table: "Orders",
                column: "ProductsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Products_ProductsId",
                table: "Orders",
                column: "ProductsId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment.Migrations
{
    /// <inheritdoc />
    public partial class v27 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Redeems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    IsLifeTime = table.Column<bool>(type: "bit", nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinimumRequirements = table.Column<double>(type: "float", nullable: false),
                    UnlimitedPercentageDiscount = table.Column<bool>(type: "bit", nullable: false),
                    MaximumPercentageReduction = table.Column<double>(type: "float", nullable: true),
                    Price = table.Column<double>(type: "float", nullable: false),
                    RankRequirement = table.Column<int>(type: "int", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redeems", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Redeems");
        }
    }
}

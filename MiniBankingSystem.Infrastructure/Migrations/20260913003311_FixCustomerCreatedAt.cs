using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniBankingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCustomerCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Customers",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Customers",
                newName: "CreateAt");
        }
    }
}

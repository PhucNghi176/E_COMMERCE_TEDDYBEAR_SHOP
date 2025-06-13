using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COMMAND.PERSISTENCE.Migrations
{
    /// <inheritdoc />
    public partial class fix_product : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ProductTags",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductTags",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "ProductTags");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductTags");
        }
    }
}

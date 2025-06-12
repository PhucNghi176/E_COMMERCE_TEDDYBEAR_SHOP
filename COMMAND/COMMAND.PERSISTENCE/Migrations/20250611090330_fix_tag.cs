using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COMMAND.PERSISTENCE.Migrations
{
    /// <inheritdoc />
    public partial class fix_tag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "Tags",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            _ = migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "Tags",
                type: "datetimeoffset",
                nullable: true);

            _ = migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tags",
                type: "bit",
                nullable: false,
                defaultValue: false);

            _ = migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "Tags",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "Tags");

            _ = migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Tags");

            _ = migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tags");

            _ = migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "Tags");
        }
    }
}

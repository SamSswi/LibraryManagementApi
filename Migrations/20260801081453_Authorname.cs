using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class Authorname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Author",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Author");
        }
    }
}

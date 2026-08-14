using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Restore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "identifier",
                table: "departments",
                newName: "slug");

            migrationBuilder.RenameIndex(
                name: "ix_departments_identifier",
                table: "departments",
                newName: "ix_departments_slug");

            migrationBuilder.AddColumn<bool>(
                name: "is_primary",
                table: "department_locations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_primary",
                table: "department_locations");

            migrationBuilder.RenameColumn(
                name: "slug",
                table: "departments",
                newName: "identifier");

            migrationBuilder.RenameIndex(
                name: "ix_departments_slug",
                table: "departments",
                newName: "ix_departments_identifier");
        }
    }
}

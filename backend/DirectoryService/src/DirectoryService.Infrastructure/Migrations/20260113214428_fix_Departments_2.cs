using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fix_Departments_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations",
                column: "location_id",
                principalTable: "locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations",
                column: "location_id",
                principalTable: "locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

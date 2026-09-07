using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentQueryIndexesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS idx_departments_name_lower_trgm
                ON departments
                USING gin (LOWER(name) gin_trgm_ops);
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS IX_department_locations_department_id
                ON department_locations (department_id);
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS IX_department_locations_location_id
                ON department_locations (location_id);
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS IX_department_positions_department_id
                ON department_positions (department_id);
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS IX_department_positions_position_id
                ON department_positions (position_id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_department_positions_position_id;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_department_positions_department_id;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_department_locations_location_id;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_department_locations_department_id;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_departments_name_lower_trgm;");
            migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
        }
    }
}

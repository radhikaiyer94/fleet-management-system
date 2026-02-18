using FleetManagementApi.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRolePostgresEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:user_role", "admin,driver,fleet_manager");

            migrationBuilder.AddColumn<UserRole>(
                name: "Role",
                table: "Users",
                type: "user_role",
                nullable: false,
                defaultValue: UserRole.Admin);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:user_role", "admin,driver,fleet_manager");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagement.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityUserIdToEmployeeTableFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_AspNetUsers_IdentityUserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "IdentityUserId",
                table: "Employees",
                newName: "AspNetUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_IdentityUserId",
                table: "Employees",
                newName: "IX_Employees_AspNetUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_AspNetUsers_AspNetUserId",
                table: "Employees",
                column: "AspNetUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_AspNetUsers_AspNetUserId",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "AspNetUserId",
                table: "Employees",
                newName: "IdentityUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_AspNetUserId",
                table: "Employees",
                newName: "IX_Employees_IdentityUserId");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_AspNetUsers_IdentityUserId",
                table: "Employees",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

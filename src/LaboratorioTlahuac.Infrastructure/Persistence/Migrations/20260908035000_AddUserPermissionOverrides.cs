using LaboratorioTlahuac.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaboratorioTlahuac.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LaboratorioTlahuacDbContext))]
[Migration("20260908035000_AddUserPermissionOverrides")]
public partial class AddUserPermissionOverrides : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserPermissionOverrides",
            schema: "Security",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Effect = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_UserPermissionOverrides",
                    x => new { x.UserId, x.PermissionId });
                table.ForeignKey(
                    name: "FK_UserPermissionOverrides_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalSchema: "Security",
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserPermissionOverrides_Users_UserId",
                    column: x => x.UserId,
                    principalSchema: "Security",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserPermissionOverrides_PermissionId",
            schema: "Security",
            table: "UserPermissionOverrides",
            column: "PermissionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserPermissionOverrides",
            schema: "Security");
    }
}

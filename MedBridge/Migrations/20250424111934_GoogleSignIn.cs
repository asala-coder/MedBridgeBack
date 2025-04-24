using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedBridge.Migrations
{
    /// <inheritdoc />
    public partial class GoogleSignIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "users");
        }
    }
}

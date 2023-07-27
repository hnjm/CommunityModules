using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleCommunity.Migrations.Migrations
{
    public partial class AddGPGProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GpgFingerprint",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GpgPublicKey",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GpgFingerprint",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GpgPublicKey",
                table: "AspNetUsers");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cura520.Migrations
{
    /// <inheritdoc />
    public partial class updateNameProbirtyandAddImg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Receptionists",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Patients",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Doctors",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Receptionists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Img",
                table: "Receptionists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Img",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Img",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "Img",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "Img",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Img",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Receptionists",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Patients",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Doctors",
                newName: "Name");
        }
    }
}

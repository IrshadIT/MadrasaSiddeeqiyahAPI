using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MadrasaSiddeeqiyahAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAadhaarVerificationToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AadhaarNumber",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AadhaarVerifiedAt",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAadhaarVerified",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AadhaarNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AadhaarVerifiedAt",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsAadhaarVerified",
                table: "Students");
        }
    }
}

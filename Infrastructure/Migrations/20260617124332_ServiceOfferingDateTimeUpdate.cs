using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ServiceOfferingDateTimeUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Availability",
                table: "ServiceOfferings");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "ServiceOfferings");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTime",
                table: "ServiceOfferings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTime",
                table: "ServiceOfferings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDateTime",
                table: "ServiceOfferings");

            migrationBuilder.DropColumn(
                name: "StartDateTime",
                table: "ServiceOfferings");

            migrationBuilder.AddColumn<string>(
                name: "Availability",
                table: "ServiceOfferings",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "ServiceOfferings",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");
        }
    }
}

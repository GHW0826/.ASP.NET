using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeTierLayer.Migrations
{
    /// <inheritdoc />
    public partial class FixThreeTierLayereed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RefreshTokenExpireTime",
                value: new DateTime(2030, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "RefreshTokenExpireTime",
                value: new DateTime(2030, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RefreshTokenExpireTime",
                value: new DateTime(2025, 5, 12, 8, 38, 9, 768, DateTimeKind.Utc).AddTicks(5447));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "RefreshTokenExpireTime",
                value: new DateTime(2025, 5, 12, 8, 38, 9, 768, DateTimeKind.Utc).AddTicks(5869));
        }
    }
}

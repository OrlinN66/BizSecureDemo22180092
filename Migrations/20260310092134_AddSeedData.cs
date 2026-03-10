using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BizSecureDemo22180092.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "Amount", "CreatedAtUtc", "Title", "UserId" },
                values: new object[,]
                {
                    { 1, 999.99m, new DateTime(2026, 3, 10, 9, 21, 34, 174, DateTimeKind.Utc).AddTicks(2877), "Premium Package", 1 },
                    { 2, 499.99m, new DateTime(2026, 3, 10, 9, 21, 34, 174, DateTimeKind.Utc).AddTicks(2886), "Standard Package", 1 },
                    { 3, 99.99m, new DateTime(2026, 3, 10, 9, 21, 34, 174, DateTimeKind.Utc).AddTicks(2888), "Basic Package", 2 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FailedLogins", "LockoutUntilUtc", "PasswordHash" },
                values: new object[,]
                {
                    { 1, "alice@test.com", null, null, "AQAAAAIAAYagAAAAEAgWtsLcyZmlgYtXvqTSk2nli3gpLLsyd9yAuHjhRuLhTbz/J+G2ITsj3SYcFf1W3w==" },
                    { 2, "bob@test.com", null, null, "AQAAAAIAAYagAAAAENNcvWlMzzHbpz9EKgEFj7bWbXolEk7EhtNEgK98yKQ95c/JICM7zzsBL3fxlH1hzw==" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}

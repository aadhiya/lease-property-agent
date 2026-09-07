using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeasePropertyAgent.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddValidationRunId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ValidationResults_LeaseId_RuleId",
                table: "ValidationResults");

            migrationBuilder.AddColumn<Guid>(
                name: "ValidationRunId",
                table: "ValidationResults",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ValidationResults_LeaseId",
                table: "ValidationResults",
                column: "LeaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationResults_ValidationRunId_RuleId",
                table: "ValidationResults",
                columns: new[] { "ValidationRunId", "RuleId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ValidationResults_LeaseId",
                table: "ValidationResults");

            migrationBuilder.DropIndex(
                name: "IX_ValidationResults_ValidationRunId_RuleId",
                table: "ValidationResults");

            migrationBuilder.DropColumn(
                name: "ValidationRunId",
                table: "ValidationResults");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationResults_LeaseId_RuleId",
                table: "ValidationResults",
                columns: new[] { "LeaseId", "RuleId" });
        }
    }
}

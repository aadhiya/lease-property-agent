using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeasePropertyAgent.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueImageAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Confidence",
                table: "IssueImages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observation",
                table: "IssueImages",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "IssueImages");

            migrationBuilder.DropColumn(
                name: "Observation",
                table: "IssueImages");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleHelpdesk.Migrations
{
    /// <inheritdoc />
    public partial class RenameRequestedAtToDateCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestedAt",
                table: "Tickets",
                newName: "DateCreation");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_InterventionId",
                table: "Tickets",
                column: "InterventionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Interventions_InterventionId",
                table: "Tickets",
                column: "InterventionId",
                principalTable: "Interventions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Interventions_InterventionId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_InterventionId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "DateCreation",
                table: "Tickets",
                newName: "RequestedAt");
        }
    }
}

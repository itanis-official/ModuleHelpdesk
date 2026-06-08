using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleHelpdesk.Migrations
{
    /// <inheritdoc />
    public partial class agentdedieattribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgentResponsableId",
                table: "Companies",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentResponsableId",
                table: "Companies");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleHelpdesk.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentIdToKnowledgeBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgentId",
                table: "KnowledgeBases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBases_AgentId",
                table: "KnowledgeBases",
                column: "AgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_KnowledgeBases_Agents_AgentId",
                table: "KnowledgeBases",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KnowledgeBases_Agents_AgentId",
                table: "KnowledgeBases");

            migrationBuilder.DropIndex(
                name: "IX_KnowledgeBases_AgentId",
                table: "KnowledgeBases");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "KnowledgeBases");
        }
    }
}

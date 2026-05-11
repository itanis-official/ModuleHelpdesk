using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleHelpdesk.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAgentIdNonIdentity : Migration
    {
        /// <inheritdoc />
       protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(name: "Agents");

    migrationBuilder.CreateTable(
        name: "Agents",
        columns: table => new
        {
            Id = table.Column<int>(nullable: false),
            Nom = table.Column<string>(nullable: false),
            Prenom = table.Column<string>(nullable: false),
            Email = table.Column<string>(nullable: false),
            Telephone = table.Column<string>(nullable: true),
            Role = table.Column<string>(nullable: false),
            Poste = table.Column<string>(nullable: false),
            Departement = table.Column<string>(nullable: false),
            IsActive = table.Column<bool>(nullable: false),
            AgentType = table.Column<string>(nullable: true),
            CoutHoraire = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            SyncedAt = table.Column<DateTime>(nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Agents", x => x.Id);
        });
}}}
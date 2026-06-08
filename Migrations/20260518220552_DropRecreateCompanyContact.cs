using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleHelpdesk.Migrations
{
    public partial class DropRecreateCompanyContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK first
            migrationBuilder.Sql(@"
                IF OBJECT_ID('FK_Contacts_Companies_CompanyId', 'F') IS NOT NULL
                    ALTER TABLE [Contacts] DROP CONSTRAINT [FK_Contacts_Companies_CompanyId]
            ");

            // ── COMPANIES ────────────────────────────────────────────
            migrationBuilder.Sql("SELECT * INTO [Companies_Backup] FROM [Companies]");
            migrationBuilder.Sql("DROP TABLE [Companies]");

            migrationBuilder.Sql(@"
                CREATE TABLE [Companies] (
                    [Id] int NOT NULL,
                    [RaisonSociale] nvarchar(max) NOT NULL,
                    [Secteur] nvarchar(max) NULL,
                    [EmailPrincipal] nvarchar(max) NULL,
                    [TelephonePrincipal] nvarchar(max) NULL,
                    [Adresse] nvarchar(max) NULL,
                    [CodePostal] nvarchar(max) NULL,
                    [Ville] nvarchar(max) NULL,
                    [Pays] nvarchar(max) NULL,
                    [MatriculeFiscal] nvarchar(max) NULL,
                    [Statut] nvarchar(max) NULL,
                    [MaxHeuresTraitementTicket] decimal(18,2) NULL,
                    [SyncedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
                )
            ");

            migrationBuilder.Sql("INSERT INTO [Companies] SELECT * FROM [Companies_Backup]");
            migrationBuilder.Sql("DROP TABLE [Companies_Backup]");

            // ── CONTACTS ─────────────────────────────────────────────
            migrationBuilder.Sql("SELECT * INTO [Contacts_Backup] FROM [Contacts]");
            migrationBuilder.Sql("DROP TABLE [Contacts]");

            migrationBuilder.Sql(@"
                CREATE TABLE [Contacts] (
                    [Id] int NOT NULL,
                    [CompanyId] int NOT NULL,
                    [Nom] nvarchar(max) NOT NULL,
                    [Prenom] nvarchar(max) NOT NULL,
                    [Poste] nvarchar(max) NULL,
                    [Email] nvarchar(max) NOT NULL,
                    [Telephone] nvarchar(max) NULL,
                    [TelephoneCountry] nvarchar(max) NULL,
                    [IsActive] bit NOT NULL,
                    [SyncedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id])
                )
            ");

            migrationBuilder.Sql("INSERT INTO [Contacts] SELECT * FROM [Contacts_Backup]");
            migrationBuilder.Sql("DROP TABLE [Contacts_Backup]");

            // Restore FK
            migrationBuilder.Sql(@"
                ALTER TABLE [Contacts]
                ADD CONSTRAINT [FK_Contacts_Companies_CompanyId]
                FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id])
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Manual rollback required.");
        }
    }
}
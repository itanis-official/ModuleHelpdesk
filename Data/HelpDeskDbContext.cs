using Microsoft.EntityFrameworkCore;
using ModuleHelpDesk.Models;
using System.Text.Json;

namespace ModuleHelpDesk.Data
{
    public class HelpDeskDbContext : DbContext
    {
        public HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Intervention> Interventions { get; set; }
        public DbSet<TicketCollaborateur> TicketCollaborateurs { get; set; }
        public DbSet<MessageTicket> MessageTickets { get; set; }
        public DbSet<KnowledgeBase> KnowledgeBases { get; set; }
        public DbSet<KnowledgeSolution> KnowledgeSolutions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversion JSON pour les listes d'URLs
            modelBuilder.Entity<Ticket>()
                .Property(t => t.ImagesUrls)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>()
                );

            modelBuilder.Entity<KnowledgeSolution>()
                .Property(s => s.PiecesJointesUrls)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>()
                );

            // Fix pour la relation Ticket <-> Message
            modelBuilder.Entity<MessageTicket>(entity =>
            {
                entity.HasOne(m => m.Ticket)
                      .WithMany(t => t.Messages)
                      .HasForeignKey(m => m.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Relation KnowledgeBase <-> Solution
            modelBuilder.Entity<KnowledgeSolution>()
                .HasOne<KnowledgeBase>()
                .WithMany(k => k.Solutions)
                .HasForeignKey(s => s.KnowledgeBaseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
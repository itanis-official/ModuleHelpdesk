using ITANIS.SharedEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModuleHelpDesk.Data;
using ModuleHelpDesk.Models;

namespace ModuleHelpdesk.Consumers;

public class CompanySyncConsumer : IConsumer<CompanySyncEvent>
{
    private readonly HelpDeskDbContext _db;
    private readonly ILogger<CompanySyncConsumer> _logger;

    public CompanySyncConsumer(HelpDeskDbContext db, ILogger<CompanySyncConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CompanySyncEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("CompanySync {Action} id={Id} {Raison}", msg.Action, msg.Id, msg.RaisonSociale);

        if (msg.Action == SyncAction.Deleted)
        {
            var existing = await _db.Companies.FindAsync(msg.Id);
            if (existing != null)
            {
                _db.Companies.Remove(existing);
                await _db.SaveChangesAsync();
            }
            return;
        }

        var entity = await _db.Companies.FindAsync(msg.Id);
        if (entity == null)
        {
            entity = new Company { Id = msg.Id };
            _db.Companies.Add(entity);
        }

        entity.RaisonSociale = msg.RaisonSociale;
        entity.Secteur = msg.Secteur;
        entity.EmailPrincipal = msg.EmailPrincipal;
        entity.TelephonePrincipal = msg.TelephonePrincipal;
        entity.Adresse = msg.Adresse;
        entity.CodePostal = msg.CodePostal;
        entity.Ville = msg.Ville;
        entity.Pays = msg.Pays;
        entity.MatriculeFiscal = msg.MatriculeFiscal;
        entity.Statut = msg.Statut;
        entity.MaxHeuresTraitementTicket = msg.MaxHeuresTraitementTicket;
        entity.SyncedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}
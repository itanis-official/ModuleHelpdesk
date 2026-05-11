using ITANIS.SharedEvents;
using MassTransit;
using ModuleHelpDesk.Data;
using ModuleHelpDesk.Models;

namespace ModuleHelpdesk.Consumers;

public class ContactSyncConsumer : IConsumer<ContactSyncEvent>
{
    private readonly HelpDeskDbContext _db;
    private readonly ILogger<ContactSyncConsumer> _logger;

    public ContactSyncConsumer(HelpDeskDbContext db, ILogger<ContactSyncConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ContactSyncEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("ContactSync {Action} id={Id} company={CompanyId}", msg.Action, msg.Id, msg.CompanyId);

        if (msg.Action == SyncAction.Deleted)
        {
            var existing = await _db.Contacts.FindAsync(msg.Id);
            if (existing != null)
            {
                _db.Contacts.Remove(existing);
                await _db.SaveChangesAsync();
            }
            return;
        }

        var entity = await _db.Contacts.FindAsync(msg.Id);
        if (entity == null)
        {
            entity = new Contact { Id = msg.Id };
            _db.Contacts.Add(entity);
        }

        entity.CompanyId = msg.CompanyId;
        entity.Nom = msg.Nom;
        entity.Prenom = msg.Prenom;
        entity.Poste = msg.Poste;
        entity.Email = msg.Email;
        entity.Telephone = msg.Telephone;
        entity.TelephoneCountry = msg.TelephoneCountry;
        entity.IsActive = msg.IsActive;
        entity.SyncedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}

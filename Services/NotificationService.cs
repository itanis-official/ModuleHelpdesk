using ModuleHelpDesk.Data;
using ModuleHelpDesk.Models;

namespace ModuleHelpDesk.Services
{
    /// <summary>
    /// Creates in-app notifications and persists them to the DB.
    /// Called from controllers whenever a relevant ticket event occurs.
    /// </summary>
    public class NotificationService
    {
        private readonly HelpDeskDbContext _db;

        public NotificationService(HelpDeskDbContext db)
        {
            _db = db;
        }

        public async Task NotifyAsync(int receiverId, ReceiverType receiverType, string subject, int? ticketId = null)
        {
            _db.Notifications.Add(new Notification
            {
                ReceiverId    = receiverId,
                ReceiverType  = receiverType,
                Subject       = subject,
                TicketId      = ticketId,
                DateCreation  = DateTime.UtcNow,
                IsRead        = false
            });
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Notifies multiple receivers in one call (e.g. company + contact + agent).
        /// </summary>
        public async Task NotifyManyAsync(IEnumerable<(int ReceiverId, ReceiverType Type)> receivers, string subject, int? ticketId = null)
        {
            foreach (var (receiverId, type) in receivers)
            {
                _db.Notifications.Add(new Notification
                {
                    ReceiverId   = receiverId,
                    ReceiverType = type,
                    Subject      = subject,
                    TicketId     = ticketId,
                    DateCreation = DateTime.UtcNow,
                    IsRead       = false
                });
            }
            await _db.SaveChangesAsync();
        }
    }
}

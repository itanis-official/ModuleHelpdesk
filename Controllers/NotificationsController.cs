using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleHelpDesk.Data;
using ModuleHelpDesk.Models;

namespace ModuleHelpDesk.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly HelpDeskDbContext _db;

        public NotificationsController(HelpDeskDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET api/notifications/receiver/{receiverId}?type=Agent
        /// Returns all notifications for a given receiver, newest first.
        /// </summary>
        [HttpGet("receiver/{receiverId}")]
        public async Task<IActionResult> GetByReceiver(int receiverId, [FromQuery] ReceiverType type)
        {
            var notifications = await _db.Notifications
                .Where(n => n.ReceiverId == receiverId && n.ReceiverType == type)
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();

            return Ok(notifications);
        }

        /// <summary>
        /// GET api/notifications/receiver/{receiverId}/unread-count?type=Agent
        /// </summary>
        [HttpGet("receiver/{receiverId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(int receiverId, [FromQuery] ReceiverType type)
        {
            var count = await _db.Notifications
                .CountAsync(n => n.ReceiverId == receiverId && n.ReceiverType == type && !n.IsRead);

            return Ok(new { unreadCount = count });
        }

        /// <summary>
        /// PATCH api/notifications/{id}/read
        /// Marks a single notification as read.
        /// </summary>
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _db.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// PATCH api/notifications/receiver/{receiverId}/read-all?type=Agent
        /// Marks all notifications of a receiver as read.
        /// </summary>
        [HttpPatch("receiver/{receiverId}/read-all")]
        public async Task<IActionResult> MarkAllAsRead(int receiverId, [FromQuery] ReceiverType type)
        {
            var unread = await _db.Notifications
                .Where(n => n.ReceiverId == receiverId && n.ReceiverType == type && !n.IsRead)
                .ToListAsync();

            unread.ForEach(n => n.IsRead = true);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

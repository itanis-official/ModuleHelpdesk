using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleHelpDesk.Data;
using ModuleHelpDesk.Models;
using ModuleHelpDesk.Services;

namespace ModuleHelpDesk.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SlaController : ControllerBase
    {
        private readonly HelpDeskDbContext _db;
        private readonly NotificationService _notif;

        public SlaController(HelpDeskDbContext db, NotificationService notif)
        {
            _db = db;
            _notif = notif;
        }

        /// <summary>
        /// POST api/sla/check
        /// Called by N8N on a schedule (e.g. every hour).
        /// Checks all open tickets against company SLA (maxHeuresTraitementTicket).
        /// - Approaching (80% of SLA): notifies agentPrincipal
        /// - Exceeded: notifies agentPrincipal + sets priority to Critique
        /// Returns a summary of actions taken.
        /// </summary>
        [HttpPost("check")]
        public async Task<IActionResult> CheckSla()
        {
            var openStatuses = new[] { StatutTicket.Nouveau, StatutTicket.EnAttente, StatutTicket.Ouvert, StatutTicket.EnPause };

            var openTickets = await _db.Tickets
                .Where(t => openStatuses.Contains(t.Statut) && t.AgentPrincipalId.HasValue)
                .ToListAsync();

            var companies = await _db.Companies
                .Where(c => c.MaxHeuresTraitementTicket.HasValue)
                .ToListAsync();

            var companyMap = companies.ToDictionary(c => c.Id);

            var approaching = new List<object>();
            var exceeded    = new List<object>();

            foreach (var ticket in openTickets)
            {
                if (!companyMap.TryGetValue(ticket.ClientId, out var company)) continue;
                if (!company.MaxHeuresTraitementTicket.HasValue) continue;

                double slaMinutes  = (double)company.MaxHeuresTraitementTicket.Value * 60;
                double elapsedMins = ticket.DureeReelleMinutes;
                double pct         = slaMinutes > 0 ? elapsedMins / slaMinutes : 0;

                // ── Exceeded SLA ────────────────────────────────────────────────
                if (elapsedMins >= slaMinutes)
                {
                    // Set priority to Critique if not already
                    if (ticket.Priorite != PrioriteTicket.Critique)
                    {
                        ticket.Priorite = PrioriteTicket.Critique;
                        _db.Tickets.Update(ticket);
                    }

                    await _notif.NotifyAsync(ticket.AgentPrincipalId!.Value, ReceiverType.Agent,
                        $"⚠️ SLA DÉPASSÉ — Ticket #{ticket.Id} « {ticket.Titre} » a dépassé le délai contractuel de {company.MaxHeuresTraitementTicket}h ({company.RaisonSociale}).",
                        ticket.Id);

                    exceeded.Add(new { ticketId = ticket.Id, clientId = ticket.ClientId, agentId = ticket.AgentPrincipalId });
                }
                // ── Approaching SLA (>= 80%) ────────────────────────────────────
                else if (pct >= 0.8)
                {
                    await _notif.NotifyAsync(ticket.AgentPrincipalId!.Value, ReceiverType.Agent,
                        $"⏰ SLA PROCHE — Ticket #{ticket.Id} « {ticket.Titre} » approche du délai contractuel de {company.MaxHeuresTraitementTicket}h ({company.RaisonSociale}). Temps écoulé : {Math.Round(elapsedMins / 60, 1)}h.",
                        ticket.Id);

                    approaching.Add(new { ticketId = ticket.Id, clientId = ticket.ClientId, agentId = ticket.AgentPrincipalId, pct = Math.Round(pct * 100, 1) });
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                checkedTickets   = openTickets.Count,
                approachingCount = approaching.Count,
                exceededCount    = exceeded.Count,
                approaching,
                exceeded
            });
        }
    }
}

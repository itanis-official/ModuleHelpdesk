using Microsoft.AspNetCore.Mvc;
using ModuleHelpDesk.Models;
using ModuleHelpDesk.Repositories;

namespace ModuleHelpDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _repo;
        public TicketsController(ITicketRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) 
        {
            var t = await _repo.GetByIdAsync(id);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClient(string clientId) => Ok(await _repo.GetByClientAsync(clientId));

        [HttpGet("agent/{agentId}")]
        public async Task<IActionResult> GetByAgent(string agentId) => Ok(await _repo.GetByAgentAsync(agentId));

        [HttpPost]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            var created = await _repo.CreateAsync(ticket);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Ticket ticket)
        {
            if (id != ticket.Id) return BadRequest();
            await _repo.UpdateAsync(ticket);
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] int newStatus)
        {
            await _repo.ChangeStatusAsync(id, newStatus);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }


        // Get by Status
[HttpGet("filter/status/{status}")]
public async Task<IActionResult> GetByStatus(StatutTicket status) 
    => Ok(await _repo.GetByStatusAsync(status));

// Get by Priority
[HttpGet("filter/priority/{priority}")]
public async Task<IActionResult> GetByPriority(PrioriteTicket priority) 
    => Ok(await _repo.GetByPriorityAsync(priority));

// Endpoint Facturation
[HttpGet("facturation")]
public async Task<IActionResult> GetFacture([FromQuery] string clientId, [FromQuery] DateTime start, [FromQuery] DateTime end)
{
    var tickets = await _repo.GetTicketsForFacturationAsync(clientId, start, end);
    return Ok(new {
        TotalTickets = tickets.Count(),
        Tickets = tickets,
        Periode = $"Du {start:dd/MM/yyyy} au {end:dd/MM/yyyy}"
    });
}
    }
}
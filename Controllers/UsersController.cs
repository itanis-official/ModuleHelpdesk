using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuleHelpDesk.Models;
using ModuleHelpDesk.Repositories;

namespace ModuleHelpDesk.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ITicketRepository _repo;
        public UsersController(ITicketRepository repo) => _repo = repo;

        #region Companies Endpoints


        [HttpGet("companies/{companyId}/dedicated-agent")]
public async Task<IActionResult> GetDedicatedAgentByCompany(int companyId)
{
    var agent = await _repo.GetDedicatedAgentByCompanyAsync(companyId);
    if (agent == null)
    {
        return NotFound($"Aucun agent dédié trouvé pour l'entreprise avec l'ID {companyId}");
    }
    return Ok(agent);
}

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies() 
            => Ok(await _repo.GetAllCompaniesAsync());

        [HttpGet("companies/{id}")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            var company = await _repo.GetCompanyByIdAsync(id);
            return company == null ? NotFound($"Entreprise avec l'ID {id} introuvable") : Ok(company);
        }

        #endregion

        #region Agents Endpoints

        [HttpGet("agents")]
        public async Task<IActionResult> GetAgents() 
            => Ok(await _repo.GetAllAgentsAsync());

        [HttpGet("agents/by-email")]
        public async Task<IActionResult> GetAgentByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");
            var agent = await _repo.GetAgentByEmailAsync(email);
            return agent == null ? NotFound($"Aucun agent trouvé avec l'email: {email}") : Ok(agent);
        }

        [HttpGet("agents/{id}")]
        public async Task<IActionResult> GetAgentById(int id)
        {
            var agent = await _repo.GetAgentByIdAsync(id);
            return agent == null ? NotFound($"Agent avec l'ID {id} introuvable") : Ok(agent);
        }

        #endregion

        #region Contacts Endpoints

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts() 
            => Ok(await _repo.GetAllContactsAsync());

        [HttpGet("companies/{companyId}/contacts")]
        public async Task<IActionResult> GetContactsByCompany(int companyId)
        {
            var contacts = await _repo.GetContactsByCompanyAsync(companyId);
            return Ok(contacts);
        }

        #endregion
    }
}
using Microsoft.AspNetCore.Mvc;
using ModuleHelpDesk.Models;
using ModuleHelpDesk.Repositories;

namespace ModuleHelpDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterventionsController : ControllerBase
    {
        private readonly ITicketRepository _repo;
        public InterventionsController(ITicketRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetInterventions() => Ok(await _repo.GetAllInterventionsAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Intervention intervention)
        {
            var created = await _repo.CreateInterventionAsync(intervention);
            return Ok(created);
        }


        [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
{
    var intervention = await _repo.GetInterventionByIdAsync(id);
    return intervention == null ? NotFound() : Ok(intervention);
}

[HttpGet("categorie/{categorie}")]
public async Task<IActionResult> GetByCategorie(int categorie)
{
    var list = await _repo.GetInterventionsByCategorieAsync(categorie);
    return Ok(list);
}

[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, Intervention intervention)
{
    if (id != intervention.Id) return BadRequest("ID mismatch");
    
    await _repo.UpdateInterventionAsync(intervention);
    return NoContent(); 
}

[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    await _repo.DeleteInterventionAsync(id);
    return NoContent();
}

    }
}
using Microsoft.AspNetCore.Mvc;
using ModuleHelpDesk.Models;
using ModuleHelpDesk.Repositories;

namespace ModuleHelpDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KnowledgeController : ControllerBase
    {
        private readonly ITicketRepository _repo;
        public KnowledgeController(ITicketRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllKnowledgeBaseAsync());

        [HttpPost]
        public async Task<IActionResult> CreateKB(KnowledgeBase kb)
        {
            var result = await _repo.CreateKnowledgeBaseAsync(kb);
            return Ok(result);
        }

        [HttpPost("solution")]
        public async Task<IActionResult> AddSolution(KnowledgeSolution solution)
        {
            var result = await _repo.AddSolutionToKbAsync(solution);
            return Ok(result);
        }

        [HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var result = await _repo.GetKnowledgeBaseByIdAsync(id);
    
    if (result == null)
    {
        return NotFound(new { message = $"Le problème avec l'ID {id} n'existe pas." });
    }

    return Ok(result);
}


        [HttpGet("categorie/{categorie}")]
public async Task<IActionResult> GetByCategorie(int categorie)
{
    var results = await _repo.GetKnowledgeByCategorieAsync(categorie);
    return Ok(results);
}


        [HttpPatch("{id}")]
public async Task<IActionResult> Patch(int id, [FromBody] KnowledgeBase kbPartiel)
{
    var existing = await _repo.GetKnowledgeBaseByIdAsync(id);
    if (existing == null) return NotFound();

    await _repo.PatchKnowledgeBaseAsync(id, kbPartiel);
    return NoContent(); // Succès, pas de contenu à renvoyer
}

[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    await _repo.DeleteKnowledgeBaseAsync(id);
    return NoContent();
}

[HttpPatch("solution/{id}")]
public async Task<IActionResult> PatchSolution(int id, [FromBody] KnowledgeSolution solPartiel)
{
    var allSolutions = await _repo.GetSolutionsByKbIdAsync(solPartiel.KnowledgeBaseId); 
    await _repo.PatchSolutionAsync(id, solPartiel);
    return NoContent();
}

[HttpDelete("solution/{id}")]
public async Task<IActionResult> DeleteSolution(int id)
{
    await _repo.DeleteSolutionAsync(id);
    return NoContent(); 
}

    }
}
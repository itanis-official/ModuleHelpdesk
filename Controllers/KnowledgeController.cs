using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ModuleHelpDesk.Models;
using ModuleHelpDesk.Repositories;
using Microsoft.AspNetCore.Http;

namespace ModuleHelpDesk.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KnowledgeController : ControllerBase
    {
        private readonly ITicketRepository _repo;
        public KnowledgeController(ITicketRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllKnowledgeBaseAsync());

        [HttpPost]
public async Task<IActionResult> CreateKB(
    [FromForm] string nomErreur,
    [FromForm] string descriptionErreur,
    [FromForm] int categorie,
    [FromForm] string initialSolutionContenu,
    [FromForm] int agentId, 
    [FromForm(Name = "files")] IEnumerable<IFormFile> files) 
{
    int currentUserId = agentId; 

    var kb = new KnowledgeBase
    {
        NomErreur = nomErreur,
        DescriptionErreur = descriptionErreur,
        Categorie = (CategorieAction)categorie,
        DateCreation = DateTime.Now,
        AgentId = currentUserId, // Lié proprement
        Solutions = new List<KnowledgeSolution>()
    };

    var initialSolution = new KnowledgeSolution
    {
        DescriptionResolution = initialSolutionContenu,
        DateResolution = DateTime.Now,
        AgentId = currentUserId, 
        PiecesJointesUrls = new List<string>()
    };

    if (files != null && files.Any())
    {
        var fileUrls = new List<string>();
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "knowledge");
        
        if (!Directory.Exists(uploadPath)) 
            Directory.CreateDirectory(uploadPath);

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                fileUrls.Add($"/uploads/knowledge/{fileName}");
            }
        }

        initialSolution.PiecesJointesUrls = fileUrls;
    }

    kb.Solutions.Add(initialSolution);

    var result = await _repo.CreateKnowledgeBaseAsync(kb);
    
    // On recharge l'objet complet avec ses relations (.Include) pour le renvoyer au Front
    var fullKb = await _repo.GetKnowledgeBaseByIdAsync(result.Id);
    return Ok(fullKb ?? result);
}

        [HttpPost("solution")]
public async Task<IActionResult> AddSolution(
    [FromForm] string descriptionResolution,
    [FromForm] int knowledgeBaseId,
    [FromForm] int agentId, 
    [FromForm(Name = "files")] IEnumerable<IFormFile> files) 
{
    try
    {
        var solution = new KnowledgeSolution
        {
            DescriptionResolution = descriptionResolution,
            KnowledgeBaseId = knowledgeBaseId,
            DateResolution = DateTime.Now,
            AgentId = agentId, 
            PiecesJointesUrls = new List<string>()
        };

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int loggedInId))
        {
            solution.AgentId = loggedInId; 
        }

        if (files != null && files.Any())
        {
            var fileUrls = new List<string>();
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "knowledge");
            
            if (!Directory.Exists(uploadPath)) 
                Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    fileUrls.Add($"/uploads/knowledge/{fileName}");
                }
            }

            solution.PiecesJointesUrls = fileUrls;
        }

        var result = await _repo.AddSolutionToKbAsync(solution);
        return Ok(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR AddSolution] : {ex.Message}");
        return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
    }
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
            return NoContent(); 
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
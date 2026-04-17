using Microsoft.EntityFrameworkCore;
using ModuleHelpDesk.Data;
using ModuleHelpDesk.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. AJOUT DES SERVICES DE BASE
builder.Services.AddControllers(); // Indispensable pour utiliser les Controllers [ApiController]
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. CONFIGURATION DU DBCONTEXT
// On récupère la chaîne de connexion depuis appsettings.json
var connectionString = builder.Configuration.GetConnectionString("HelpDeskConnection");

builder.Services.AddDbContext<HelpDeskDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. INJECTION DE DÉPENDANCE POUR LE REPOSITORY
// Cela permet au Controller de recevoir l'interface ITicketRepository
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

var app = builder.Build();

// 4. CONFIGURATION DU PIPELINE (Middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Supprime ou commente UseHttpsRedirection si tu as des problèmes de certificats en local/Docker
// app.UseHttpsRedirection();

app.UseAuthorization(); // Important pour plus tard

// 5. MAPPAGE DES CONTROLLERS
app.MapControllers(); // C'est cette ligne qui va chercher ton TicketsController

app.Run();
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModuleHelpDesk.Data;
using ModuleHelpDesk.Repositories;
using ModuleHelpdesk.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173"
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("HelpDeskConnection");
builder.Services.AddDbContext<HelpDeskDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ITicketRepository, TicketRepository>();

builder.Services.AddMassTransit(x =>
{
    // Register your consumers
    x.AddConsumer<AgentSyncConsumer>();
    x.AddConsumer<CompanySyncConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("51.254.133.231", 31672, "/", h =>
        {
            h.Username("admin");
            h.Password("rabbitMQ-dev");
        });

        // Each consumer needs its own receive endpoint
        cfg.ReceiveEndpoint("helpdesk-agent-sync", e =>
        {
            e.ConfigureConsumer<AgentSyncConsumer>(ctx);
        });

        cfg.ReceiveEndpoint("helpdesk-company-sync", e =>
        {
            e.ConfigureConsumer<CompanySyncConsumer>(ctx);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ─── Middleware order matters ──────────────────────────────────────────────────
app.UseCors("AllowFrontend");   // must be before UseAuthorization
app.UseAuthorization(); 
app.MapControllers(); 

app.Run();
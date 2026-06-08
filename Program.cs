using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModuleHelpDesk.Data;
using ModuleHelpDesk.Repositories;
using ModuleHelpdesk.Consumers;
// ─── Directives requises pour l'authentification Authentik ──────────────────
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

// ─── Configuration de l'authentification JwtBearer (Authentik) ──────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://authentik.itanis.tn/application/o/erp-application/";
        options.Audience = "BGnXFXMepfj4wh0AVli40YPWPjTFs9SgBxf1Udxk";
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("HelpDeskConnection");
builder.Services.AddDbContext<HelpDeskDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ITicketRepository, TicketRepository>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AgentSyncConsumer>();
    x.AddConsumer<CompanySyncConsumer>();
    x.AddConsumer<ContactSyncConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("51.254.133.231", 31672, "/", h =>
        {
            h.Username("admin");
            h.Password("rabbitMQ-dev");
        });

        cfg.ReceiveEndpoint("helpdesk-agent-sync", e =>
        {
            e.ConfigureConsumer<AgentSyncConsumer>(ctx);
        });

        cfg.ReceiveEndpoint("helpdesk-contact-sync", e =>
        {
            e.ConfigureConsumer<ContactSyncConsumer>(ctx);
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

app.UseCors("AllowFrontend");       // 1. Gère les requêtes Cross-Origin d'abord
app.UseAuthentication();           // 2. Extrait et valide le Token Authentik (Ajouté)
app.UseAuthorization();            // 3. Vérifie les droits d'accès aux routes

app.UseStaticFiles();
app.MapControllers(); 

app.Run();
using LeasePropertyAgent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Infrastructure.DocumentExtraction;
using LeasePropertyAgent.Infrastructure.LeaseAgents;
using LeasePropertyAgent.Application.Services;
using LeasePropertyAgent.Infrastructure.Repositories;
using LeasePropertyAgent.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDocumentExtractor, StubDocumentExtractor>();
builder.Services.AddDbContext<LeasePropertyDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ILeaseAgent, StubLeaseAgent>();
builder.Services.AddScoped<IUnitMatchingService, UnitMatchingService>();
builder.Services.AddScoped<ILeaseProcessingService, LeaseProcessingService>();
builder.Services.AddScoped<ILeaseRepository, LeaseRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
var dataPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "data"));

builder.Services.AddScoped<IUnitCatalogProvider>(_ =>
    new JsonUnitCatalogProvider(
        Path.Combine(dataPath, "units.json")));

builder.Services.AddScoped<IOwnerRulesetProvider>(_ =>
    new JsonOwnerRulesetProvider(
        Path.Combine(dataPath, "owner_ruleset.json")));

builder.Services.AddScoped<IPropertyCatalogSeeder, JsonPropertyCatalogSeeder>();
builder.Services.AddScoped<ILeaseValidationService, LeaseValidationService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
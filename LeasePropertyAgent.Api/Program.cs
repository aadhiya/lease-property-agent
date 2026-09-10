using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Services;
using LeasePropertyAgent.Infrastructure.Data;
using LeasePropertyAgent.Infrastructure.DocumentExtraction;
using LeasePropertyAgent.Infrastructure.IssueAgents;
using LeasePropertyAgent.Infrastructure.LeaseAgents;
using LeasePropertyAgent.Infrastructure.Providers;
using LeasePropertyAgent.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LeasePropertyDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDocumentExtractor, StubDocumentExtractor>();

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

builder.Services.AddScoped<
    IPropertyCatalogSeeder,
    JsonPropertyCatalogSeeder>();

builder.Services.AddScoped<ILeaseValidationService, LeaseValidationService>();

builder.Services.AddScoped<IIssueAgent, StubIssueAgent>();
builder.Services.AddScoped<IIssueProcessingService, IssueProcessingService>();
builder.Services.AddScoped<IIssueRepository, IssueRepository>();

builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddScoped<IUnitWorkspaceService, UnitWorkspaceService>();
builder.Services.AddScoped<IUnitService, UnitService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Make sure the development database exists
// and synchronize the property catalog from units.json.
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<LeasePropertyDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        var seeder =
            scope.ServiceProvider
                .GetRequiredService<IPropertyCatalogSeeder>();

        await seeder.SeedAsync();
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("Frontend");

app.MapControllers();

app.Run();

public partial class Program
{
}
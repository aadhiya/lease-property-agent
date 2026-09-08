using LeasePropertyAgent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Infrastructure.DocumentExtraction;
using LeasePropertyAgent.Infrastructure.LeaseAgents;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDocumentExtractor, StubDocumentExtractor>();
builder.Services.AddDbContext<LeasePropertyDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ILeaseAgent, StubLeaseAgent>();
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
using TryalTestForMargasoftBusinessLogic.Interfaces;
using TryalTestForMargasoftBusinessLogic.Services;
using TryalTestForMargasoftInfrastrcture.Repositories;
using TryalTestForMargsoftCore.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IMedicalClaimRepository, InMemoryMedicalClaimRepository>();
builder.Services.AddSingleton<IClaimRecommendationRepository, InMemoryClaimRecommendationRepository>();
builder.Services.AddScoped<IClaimWorkflowService, ClaimWorkflowService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

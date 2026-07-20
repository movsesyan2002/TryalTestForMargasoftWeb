using TryalTestForMargasoftBusinessLogic.Interfaces;
using TryalTestForMargasoftBusinessLogic.Mappings;
using TryalTestForMargasoftBusinessLogic.Services;
using TryalTestForMargasoftInfrastrcture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured."));

builder.Services.AddAutoMapper(configuration =>
    configuration.AddProfile<MedicalClaimMappingProfile>());

builder.Services.AddScoped<IClaimWorkflowService, ClaimWorkflowService>();
builder.Services.AddScoped<IClaimLookupService, ClaimLookupService>();

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

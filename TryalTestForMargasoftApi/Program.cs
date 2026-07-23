using Microsoft.Extensions.FileProviders;
using TryalTestForMargasoftBusinessLogic.Interfaces;
using TryalTestForMargasoftBusinessLogic.Mappings;
using TryalTestForMargasoftBusinessLogic.Services;
using TryalTestForMargasoftInfrastrcture;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5025", "https://localhost:7022")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured."));

builder.Services.AddAutoMapper(configuration =>
    configuration.AddProfile<MedicalClaimMappingProfile>());

builder.Services.AddScoped<IClaimWorkflowService, ClaimWorkflowService>();
builder.Services.AddScoped<IClaimLookupService, ClaimLookupService>();

var app = builder.Build();

var clientBuildConfiguration = app.Configuration["ClientBuildConfiguration"]
    ?? (app.Environment.IsDevelopment() ? "Debug" : "Release");
var clientSourceWebRoot = Path.GetFullPath(Path.Combine(
    app.Environment.ContentRootPath,
    "..",
    "TryalTestForMargasoft",
    "wwwroot"));
var clientBuildWebRoot = Path.GetFullPath(Path.Combine(
    app.Environment.ContentRootPath,
    "..",
    "TryalTestForMargasoft",
    "bin",
    clientBuildConfiguration,
    "net10.0",
    "wwwroot"));
var clientScopedCssRoot = Path.GetFullPath(Path.Combine(
    app.Environment.ContentRootPath,
    "..",
    "TryalTestForMargasoft",
    "obj",
    clientBuildConfiguration,
    "net10.0",
    "scopedcss",
    "bundle"));

if (Directory.Exists(clientSourceWebRoot) && Directory.Exists(clientBuildWebRoot))
{
    var clientFileProviders = new List<IFileProvider>
    {
        new PhysicalFileProvider(clientSourceWebRoot),
        new PhysicalFileProvider(clientBuildWebRoot)
    };

    if (Directory.Exists(clientScopedCssRoot))
    {
        clientFileProviders.Add(new PhysicalFileProvider(clientScopedCssRoot));
    }

    var clientFiles = new CompositeFileProvider(clientFileProviders);

    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = clientFiles });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = clientFiles });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("BlazorClient");

app.UseAuthorization();

app.MapControllers();

if (Directory.Exists(clientSourceWebRoot) && Directory.Exists(clientBuildWebRoot))
{
    app.MapFallback(async context =>
    {
        if (context.Request.Path.StartsWithSegments("/api")
            || context.Request.Path.StartsWithSegments("/swagger"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = "text/html";
        var html = await File.ReadAllTextAsync(Path.Combine(clientSourceWebRoot, "index.html"));
        var blazorBootScript = Directory
            .GetFiles(Path.Combine(clientBuildWebRoot, "_framework"), "blazor.webassembly.*.js")
            .Select(Path.GetFileName)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(blazorBootScript))
        {
            html = html.Replace(
                "_framework/blazor.webassembly#[.{fingerprint}].js",
                $"_framework/{blazorBootScript}");
        }

        await context.Response.WriteAsync(html);
    });
}

app.Run();

using System.Text.Json.Serialization;
using Application.DermaImage;
using Infrastructure.DermaImage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.DermaImage.Middleware;
using WebApi.DermaImage.Managers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("WebApi.Validation");

        var validationErrors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => new
            {
                Field = x.Key,
                Errors = x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            })
            .ToArray();

        logger.LogWarning(
            "Validation FAILED [{TraceId}] {Method} {Path}. Errors: {@ValidationErrors}",
            context.HttpContext.TraceIdentifier,
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            validationErrors);

        return new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
    };
});

builder.Services.AddOpenApi();

builder.Services.AddAuthorization(options =>
{
    // Secure-by-default: every endpoint requires an authenticated user
    // unless it is explicitly marked with [AllowAnonymous].
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Add CORS for Blazor client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7262", "http://localhost:5262")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Register layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IImageUploadManager, ImageUploadManager>();

var app = builder.Build();

app.Logger.LogInformation("Starting API in {Environment} environment", app.Environment.EnvironmentName);

// Apply pending migrations automatically
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DermaImageDbContext>();
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("Database migration check completed successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "Database migration failed during startup");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseApiRequestLogging();
app.UseCors("AllowBlazorClient");
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

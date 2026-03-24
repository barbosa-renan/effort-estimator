using System.Reflection;
using System.Text.Json;
using EffortEstimator.API.Middleware;
using EffortEstimator.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "EffortEstimator API",
        Version     = "v1",
        Description = "PERT-based software effort estimation API. " +
                      "Accepts a task description and returns estimated hours, " +
                      "Story Points (Fibonacci scale), standard deviation, " +
                      "confidence interval, and risk level based on the PERT formula.",
        Contact = new OpenApiContact
        {
            Name  = "EffortEstimator Team",
            Email = "team@effortestimator.dev"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EffortEstimator API v1");
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseExceptionHandler();
app.MapControllers();

app.Run();

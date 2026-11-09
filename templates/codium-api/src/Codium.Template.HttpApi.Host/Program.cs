using System.Text.Json;
using System.Text.Json.Serialization;
using Codium.Template.Application;
using Codium.Template.EntityFrameworkCore;
using Codium.Template.HttpApi.Attributes;
using Codium.Template.HttpApi.Client;
using Codium.Template.HttpApi.Host.Configuration;
using Codium.Template.HttpApi.Host.Logging;
using Codium.Template.HttpApi.Host.Middlewares;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddRateLimitingConfiguration(builder.Configuration);

builder.Services.AddOpenApi();

builder.Services.AddEntityFrameworkCoreService(builder.Configuration);
builder.Services.AddApplicationService(builder.Configuration);
builder.Services.AddHttpApiClientExtensions();

builder.AddSerilogLogging(builder.Configuration);

var app = builder.Build();

// Run migrations before app.Run() to guarantee DB exists before Hangfire initializes its storage
await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbMigrationInitializer>();
    await initializer.InitializeAsync();
}

app.UseCustomMiddlewares();

app.MapControllers();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHangfireDashboard(builder.Configuration["Hangfire:Url"]!, new DashboardOptions
{
    Authorization =
    [
        new HangfireCustomBasicAuthenticationFilter
        {
            User = builder.Configuration["Hangfire:Username"],
            Pass = builder.Configuration["Hangfire:Password"]
        }
    ],
    DisplayStorageConnectionString = false,
    DashboardTitle = builder.Configuration["Hangfire:Title"],
    DarkModeEnabled = true
});

app.Run();

public partial class Program { }
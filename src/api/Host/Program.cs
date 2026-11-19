using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PropertyPlatform.Host;
using PropertyPlatform.Host.Auth;
using PropertyPlatform.SharedKernel.Persistence;
using PropertyPlatform.SharedKernel.Tenancy;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ---- Observability: Application Insights via OpenTelemetry (only when configured) ----
if (!string.IsNullOrWhiteSpace(config["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

// ---- Platform services ----
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddScoped<OrgContext>();
builder.Services.AddScoped<IOrgContext>(sp => sp.GetRequiredService<OrgContext>());
builder.Services.AddScoped<OrgRlsConnectionInterceptor>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

// ---- Authentication: Entra External ID (JWT) in every real environment ----
var authority = config["Auth:Authority"];
if (string.IsNullOrWhiteSpace(authority))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException("Auth:Authority must be configured outside Development.");
    }

    builder.Services.AddAuthentication(DevAuthHandler.SchemeName)
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, null);
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            o.Authority = authority;
            o.Audience = config["Auth:Audience"];
            o.MapInboundClaims = false;
        });
}

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireClaim(OrgClaims.OrgId).Build());

// ---- Health checks ----
var connectionString = config.GetConnectionString("Platform");
var health = builder.Services.AddHealthChecks();
if (!string.IsNullOrWhiteSpace(connectionString))
{
    health.AddNpgSql(connectionString, name: "postgres", tags: ["ready"]);
}

// ---- Modules ----
foreach (var module in ModuleCatalog.All)
{
    module.RegisterServices(builder.Services, config);
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();

// Resolve the organisation for this request from the authenticated user's org_id claim.
app.Use(async (ctx, next) =>
{
    if (Guid.TryParse(ctx.User.FindFirst(OrgClaims.OrgId)?.Value, out var orgId))
    {
        ctx.RequestServices.GetRequiredService<OrgContext>().Set(orgId);
    }

    await next(ctx);
});

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") }).AllowAnonymous();

foreach (var module in ModuleCatalog.All)
{
    module.MapEndpoints(app);
}

await app.RunAsync();

public partial class Program;

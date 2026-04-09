using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NexusITSM.Components;
using NexusITSM.Data;
using NexusITSM.Hubs;
using NexusITSM.Models.Entities;
using NexusITSM.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication (for API)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "NexusITSM-SuperSecret-Key-Min32Chars!";
builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "NexusITSM",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "NexusITSM",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    // Don't challenge API requests with redirects
    options.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

// Services
builder.Services.AddSingleton<DemoDataService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<ProblemService>();
builder.Services.AddScoped<ChangeService>();
builder.Services.AddScoped<CmdbService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddSingleton<EmailGrabberService>();
builder.Services.AddSingleton<WebhookService>();
builder.Services.AddSingleton<WorkflowEngine>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<SlaBackgroundService>();

// SignalR
builder.Services.AddSignalR();

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Seed database
await DbSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapAuthEndpoints();
app.MapJwtEndpoints();
app.MapExportEndpoints();
app.MapApiEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

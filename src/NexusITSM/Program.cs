using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
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
builder.Services.AddSingleton<EmailGrabberService>();
builder.Services.AddSingleton<WebhookService>();
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
app.MapExportEndpoints();
app.MapApiEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

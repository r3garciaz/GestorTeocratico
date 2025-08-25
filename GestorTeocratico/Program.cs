using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GestorTeocratico.Components;
using GestorTeocratico.Components.Account;
using GestorTeocratico.Data;
using QuestPDF.Infrastructure;
using GestorTeocratico.Features.Congregations;
using GestorTeocratico.Features.Departments;
using GestorTeocratico.Features.Publishers;
using GestorTeocratico.Features.Responsibilities;
using GestorTeocratico.Features.PublisherResponsibilities;
using GestorTeocratico.Features.MeetingSchedules;
using GestorTeocratico.Features.MeetingSchedules.Endpoints;
using GestorTeocratico.Features.ResponsibilityAssignments;
using Radzen;
using Resend;

using GestorTeocratico.Features.PdfExport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using GestorTeocratico.Services;
using Microsoft.AspNetCore.HttpOverrides;

// Configure QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "RadzenBlazorStudioTheme";
    options.Duration = TimeSpan.FromDays(365);
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options => {
    options.ExpireTimeSpan = TimeSpan.FromDays(5);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(option =>
{
    option.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.AddScoped<ICongregationService, CongregationService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IResponsibilityService, ResponsibilityService>();
builder.Services.AddScoped<IPublisherResponsibilityService, PublisherResponsibilityService>();
builder.Services.AddScoped<IMeetingScheduleService, MeetingScheduleService>();
builder.Services.AddScoped<IResponsibilityAssignmentService, ResponsibilityAssignmentService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<GestorTeocratico.Features.Roles.IRoleService, GestorTeocratico.Features.Roles.RoleService>();

// Configure Resend Email Service
var resendApiToken = builder.Configuration["RESEND:APITOKEN"];
if (!string.IsNullOrWhiteSpace(resendApiToken))
{
    builder.Services.AddHttpClient<ResendClient>();
    builder.Services.Configure<ResendClientOptions>(o =>
    {
        o.ApiToken = resendApiToken!;
    });
    builder.Services.AddTransient<IResend, ResendClient>();
    builder.Services.AddScoped<IEmailSender<ApplicationUser>, ResendEmailSender>();
}
else
{
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
}
// Configure HttpClient for PDF downloads
builder.Services.AddHttpClient<PdfExportHttpClient>(client =>
{
    // This will be the base URL for the same application
    // In production, you might want to configure this differently
    client.BaseAddress = new Uri("https://localhost:7095"); // Adjust as needed
});

// Add this after var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    try
    {
        logger.LogInformation("Applying migrations in development environment...");
        await dbContext.Database.MigrateAsync();
        await DataSeederDevelopment.SeedDataAsync(dbContext);

    }
    catch (Exception e)
    {
        logger.LogError("An error occurred while migrating or seeding the database. {Error}", e.Message);
        throw;
    }
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    
    try
    {
        logger.LogInformation("Applying migrations in production environment...");
        await dbContext.Database.MigrateAsync();
        await DataSeeder.SeedDataAsync(dbContext);
    }
    catch (Exception e)
    {
        logger.LogError("An error occurred while migrating the database. {Error}", e.Message);
        throw;
    }
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets().AllowAnonymous();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapGroup("/api")
    .MapMeetingSchedulesEndpoints();

app.Run();
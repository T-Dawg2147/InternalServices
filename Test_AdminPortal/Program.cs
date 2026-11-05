using InternalServices.Auth.Admin;
using InternalServices.Auth.Web;
using Test_AdminPortal.Components;
using Test_AdminPortal.Services;

var builder = WebApplication.CreateBuilder(args);

var portalCatalog = builder.Configuration
    .GetSection("PortalCatalogue")
    .Get<PortalCatalogOptions>() ?? new PortalCatalogOptions();

portalCatalog.Validate();
builder.Services.AddSingleton(portalCatalog);

var useDevAuth = builder.Environment.IsDevelopment();

builder.Services.AddInternalAuthWeb(
    builder.Configuration,
    o =>
    {
        o.ToolKey = "admin-portal";
        o.ConnectionString = builder.Configuration.GetConnectionString("InternalServices") 
                             ?? throw new ArgumentException("Connection string missing or invalid.");
        o.PermissionCacheMinutes = 5;
    },
    useDevelopmentAuth: useDevAuth);

builder.Services.AddScoped<UserContext>();

builder.Services.AddInternalServicesAuthAdmin();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
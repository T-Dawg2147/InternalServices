using InternalServices.Auth.Core;
using InternalServices.Auth.Core.Abstractions;
using InternalServices.Auth.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace InternalServices.Auth.Web;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// One call that sets up: Entra sign-in, the database-backed permission service,
    /// permission claims, and [RequirePermission] policies.
    /// </summary>
    /// <param name="configuration">The app's configuration (appsettings.json etc.).</param>
    /// <param name="configure">Sets ToolKey, ConnectionString, etc.</param>
    /// <param name="azureAdSection">Config section holding TenantId / ClientId.</param>
    /// <param name="useDevelopmentAuth">Specifies if this is currently being designed for live use.</param>
    public static IServiceCollection AddInternalAuthWeb(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<AuthOptions> configure,
        bool useDevelopmentAuth = false,
        string azureAdSection = "AzureAd")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddInternalServicesAuthData(configure);

        if (useDevelopmentAuth)
        {
            services
                .AddAuthentication(DevelopmentAuthenticationDefaults.Schema)
                .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
                    DevelopmentAuthenticationDefaults.Schema, _ => { });
        }
        else
        {
            services
                .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(configuration.GetSection(azureAdSection));
        }

        services.AddAuthorization(o =>
        {
            o.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        services.AddHttpContextAccessor();
        services.AddTransient<IClaimsTransformation, PermissionClaimsTransformation>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        return services;
    }
}
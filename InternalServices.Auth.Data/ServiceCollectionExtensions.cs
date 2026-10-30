using InternalServices.Auth.Core;
using InternalServices.Auth.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InternalServices.Auth.Data;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SQL-backed user repository and permission service.
    /// </summary>
    public static IServiceCollection AddInternalServicesAuthData(
        this IServiceCollection services,
        Action<AuthOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new AuthOptions();
        configure(options);
        options.Validate();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new InvalidOperationException($"{nameof(AuthOptions.ConnectionString)} must be set.");

        services.AddSingleton(options);
        services.AddMemoryCache();
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IPermissionService, PermissionService>();

        return services;
    }
}
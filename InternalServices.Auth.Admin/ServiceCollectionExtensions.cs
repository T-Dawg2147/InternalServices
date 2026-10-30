using InternalServices.Auth.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InternalServices.Auth.Admin;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInternalServicesAuthAdmin(this IServiceCollection services)
    {
        services.AddSingleton<IAdminService, AdminService>();
        return services;
    }
}
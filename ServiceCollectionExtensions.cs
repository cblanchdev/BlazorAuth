using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAuth;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorAuth(this IServiceCollection services, Action<BlazorAuthOptions> configure)
    {
        return services.AddScoped<AuthenticationStateProvider, BlazorAuth>().Configure<BlazorAuthOptions>(options => { configure?.Invoke(options); });
    }
}

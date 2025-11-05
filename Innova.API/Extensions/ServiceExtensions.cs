using Innova.Application.Services.Implementations;
using Innova.Application.Services.Interfaces;

namespace Innova.API.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

    public static void ConfigureServices(this IServiceCollection services)
    {
        // Add Mapster configuration
        // services.AddMappings(); to be reviewed
        services.AddScoped<IAuthService, AuthService>();
    }
}

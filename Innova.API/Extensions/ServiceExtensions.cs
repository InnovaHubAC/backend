using Innova.Application.MappingConfig;

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
        // services.AddMappings(); to be reviewe
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IIdeaService, IdeaService>();
        services.AddScoped<IUsersService, UsersService>();
        // Add Mapster configuration
        MappingConfig.ConfigureMappings();
    }
}

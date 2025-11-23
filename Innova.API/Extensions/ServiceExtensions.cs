using Innova.Application.MappingConfig;
using Innova.Application.Services.Implementations;
using Innova.Application.Services.Interfaces;

namespace Innova.API.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        // services.AddMappings(); to be reviewe
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IIdeaService, IdeaService>();
        services.AddScoped<ICommentService, CommentService>();
        // Add Mapster configuration
        MappingConfig.ConfigureMappings();
    }
}


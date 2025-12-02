using Innova.Application.MappingConfig;

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
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IMessagingService, MessagingService>();
        
        // Add Mapster configuration
        MappingConfig.ConfigureMappings();
    }
}

